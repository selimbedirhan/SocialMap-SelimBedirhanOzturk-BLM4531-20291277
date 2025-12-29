using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.DTOs;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;

namespace SocialMap.WebAPI.Controllers;

/// <summary>
/// Admin paneli için API endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IReportRepository _reportRepository;

    public AdminController(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IReportRepository reportRepository)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _reportRepository = reportRepository;
    }

    /// <summary>
    /// Admin istatistikleri
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> GetStats()
    {
        var today = DateTime.UtcNow.Date;
        
        var users = await _userRepository.GetAllAsync();
        var usersList = users.ToList();
        
        var posts = await _postRepository.GetAllAsync();
        var postsList = posts.ToList();
        
        var comments = await _commentRepository.GetAllAsync();
        
        var pendingReports = await _reportRepository.GetPendingReportsAsync();

        var stats = new AdminStatsDto
        {
            TotalUsers = usersList.Count,
            TotalPosts = postsList.Count,
            TotalComments = comments.Count(),
            PendingReports = pendingReports.Count(),
            BannedUsers = usersList.Count(u => u.IsBanned),
            NewUsersToday = usersList.Count(u => u.CreatedAt >= today),
            NewPostsToday = postsList.Count(p => p.CreatedAt >= today)
        };

        return Ok(stats);
    }

    /// <summary>
    /// Tüm raporları sayfalı getir
    /// </summary>
    [HttpGet("reports")]
    public async Task<ActionResult<PaginatedResponse<ReportResponseDto>>> GetReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var (items, totalCount) = await _reportRepository.GetReportsPagedAsync(page, pageSize, status);
        
        var reportDtos = items.Select(r => new ReportResponseDto
        {
            Id = r.Id,
            ReporterId = r.ReporterId,
            ReporterUsername = r.Reporter?.Username,
            TargetType = r.TargetType,
            TargetId = r.TargetId,
            Reason = r.Reason,
            Description = r.Description,
            Status = r.Status,
            AdminNotes = r.AdminNotes,
            ReviewedByUserId = r.ReviewedByUserId,
            ReviewedByUsername = r.ReviewedByUser?.Username,
            CreatedAt = r.CreatedAt,
            ReviewedAt = r.ReviewedAt
        });

        return Ok(new PaginatedResponse<ReportResponseDto>
        {
            Items = reportDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Rapor durumunu güncelle
    /// </summary>
    [HttpPut("reports/{id}")]
    public async Task<IActionResult> UpdateReportStatus(Guid id, [FromBody] UpdateReportStatusDto dto, [FromQuery] Guid adminUserId)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
            return NotFound("Rapor bulunamadı");

        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin == null || !admin.IsAdmin)
            return Forbid("Bu işlem için admin yetkisi gerekli");

        report.Status = dto.Status;
        report.AdminNotes = dto.AdminNotes;
        report.ReviewedByUserId = adminUserId;
        report.ReviewedAt = DateTime.UtcNow;

        await _reportRepository.UpdateAsync(report);
        return Ok(new { message = "Rapor güncellendi" });
    }

    /// <summary>
    /// Kullanıcıyı yasakla
    /// </summary>
    [HttpPost("users/{userId}/ban")]
    public async Task<IActionResult> BanUser(Guid userId, [FromQuery] Guid adminUserId, [FromBody] BanUserDto dto)
    {
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin == null || !admin.IsAdmin)
            return Forbid("Bu işlem için admin yetkisi gerekli");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return NotFound("Kullanıcı bulunamadı");

        user.IsBanned = true;
        user.BanReason = dto.Reason;

        await _userRepository.UpdateAsync(user);
        return Ok(new { message = $"{user.Username} kullanıcısı yasaklandı" });
    }

    /// <summary>
    /// Kullanıcı yasağını kaldır
    /// </summary>
    [HttpPost("users/{userId}/unban")]
    public async Task<IActionResult> UnbanUser(Guid userId, [FromQuery] Guid adminUserId)
    {
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin == null || !admin.IsAdmin)
            return Forbid("Bu işlem için admin yetkisi gerekli");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return NotFound("Kullanıcı bulunamadı");

        user.IsBanned = false;
        user.BanReason = null;

        await _userRepository.UpdateAsync(user);
        return Ok(new { message = $"{user.Username} kullanıcısının yasağı kaldırıldı" });
    }

    /// <summary>
    /// Kullanıcıya admin yetkisi ver
    /// </summary>
    [HttpPost("users/{userId}/make-admin")]
    public async Task<IActionResult> MakeAdmin(Guid userId, [FromQuery] Guid adminUserId)
    {
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin == null || !admin.IsAdmin)
            return Forbid("Bu işlem için admin yetkisi gerekli");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return NotFound("Kullanıcı bulunamadı");

        user.IsAdmin = true;
        await _userRepository.UpdateAsync(user);
        return Ok(new { message = $"{user.Username} artık admin" });
    }

    /// <summary>
    /// Gönderiyi sil (Admin)
    /// </summary>
    [HttpDelete("posts/{postId}")]
    public async Task<IActionResult> DeletePost(Guid postId, [FromQuery] Guid adminUserId)
    {
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin == null || !admin.IsAdmin)
            return Forbid("Bu işlem için admin yetkisi gerekli");

        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            return NotFound("Gönderi bulunamadı");

        await _postRepository.DeleteAsync(post);
        return Ok(new { message = "Gönderi silindi" });
    }

    /// <summary>
    /// Yorumu sil (Admin)
    /// </summary>
    [HttpDelete("comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid commentId, [FromQuery] Guid adminUserId)
    {
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin == null || !admin.IsAdmin)
            return Forbid("Bu işlem için admin yetkisi gerekli");

        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            return NotFound("Yorum bulunamadı");

        await _commentRepository.DeleteAsync(comment);
        return Ok(new { message = "Yorum silindi" });
    }
}

public class BanUserDto
{
    public string? Reason { get; set; }
}
