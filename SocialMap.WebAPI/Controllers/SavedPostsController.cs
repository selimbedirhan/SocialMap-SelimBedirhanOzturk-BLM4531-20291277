using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.DTOs;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using AutoMapper;

namespace SocialMap.WebAPI.Controllers;

/// <summary>
/// Kaydedilen gönderiler (Favoriler) için API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SavedPostsController : ControllerBase
{
    private readonly ISavedPostRepository _savedPostRepository;
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;

    public SavedPostsController(
        ISavedPostRepository savedPostRepository,
        IPostRepository postRepository,
        IMapper mapper)
    {
        _savedPostRepository = savedPostRepository;
        _postRepository = postRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Kullanıcının kayıtlı gönderilerini getir
    /// </summary>
    [HttpGet("user/{userId}")]
    [AllowAnonymous] // Profil görüntülemek için auth gerekmesin
    public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetSavedPosts(Guid userId)
    {
        var savedPosts = await _savedPostRepository.GetByUserIdAsync(userId);
        
        var postDtos = savedPosts
            .Where(sp => sp.Post != null)
            .Select(sp => new PostResponseDto
            {
                Id = sp.Post!.Id,
                UserId = sp.Post.UserId,
                Username = sp.Post.User?.Username ?? "Bilinmiyor",
                UserProfilePhotoUrl = sp.Post.User?.ProfilePhotoUrl,
                PlaceId = sp.Post.PlaceId,
                PlaceName = sp.Post.PlaceName ?? sp.Post.Place?.Name ?? "",
                PlaceLocation = BuildPlaceLocation(sp.Post),
                Latitude = sp.Post.Latitude,
                Longitude = sp.Post.Longitude,
                MediaUrl = sp.Post.MediaUrl,
                Caption = sp.Post.Caption,
                LikesCount = sp.Post.LikesCount,
                CommentsCount = sp.Post.CommentsCount,
                CreatedAt = sp.Post.CreatedAt,
                Comments = new List<CommentResponseDto>(), // Boş bırak - circular ref önleme
                Likes = new List<LikeResponseDto>()
            })
            .ToList();
            
        return Ok(postDtos);
    }
    
    private static string BuildPlaceLocation(Post post)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(post.PlaceName)) parts.Add(post.PlaceName);
        if (!string.IsNullOrEmpty(post.City)) parts.Add(post.City);
        if (!string.IsNullOrEmpty(post.Country)) parts.Add(post.Country);
        return parts.Count > 0 ? string.Join(" - ", parts) : "Konum belirtilmemiş";
    }

    /// <summary>
    /// Gönderiyi kaydet
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SavePost([FromBody] SavePostDto dto)
    {
        var post = await _postRepository.GetByIdAsync(dto.PostId);
        if (post == null)
            return NotFound("Gönderi bulunamadı");

        var existing = await _savedPostRepository.GetByUserAndPostAsync(dto.UserId, dto.PostId);
        if (existing != null)
            return BadRequest("Bu gönderi zaten kayıtlı");

        var savedPost = new SavedPost
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            PostId = dto.PostId,
            SavedAt = DateTime.UtcNow
        };

        await _savedPostRepository.AddAsync(savedPost);
        return Ok(new { message = "Gönderi kaydedildi" });
    }

    /// <summary>
    /// Gönderiyi kayıtlardan kaldır
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> UnsavePost([FromQuery] Guid userId, [FromQuery] Guid postId)
    {
        var savedPost = await _savedPostRepository.GetByUserAndPostAsync(userId, postId);
        if (savedPost == null)
            return NotFound("Kayıtlı gönderi bulunamadı");

        await _savedPostRepository.DeleteAsync(savedPost);
        return Ok(new { message = "Gönderi kayıtlardan kaldırıldı" });
    }

    /// <summary>
    /// Gönderi kayıtlı mı kontrol et
    /// </summary>
    [HttpGet("check")]
    public async Task<ActionResult<bool>> IsSaved([FromQuery] Guid userId, [FromQuery] Guid postId)
    {
        var isSaved = await _savedPostRepository.IsSavedAsync(userId, postId);
        return Ok(isSaved);
    }
}

public class SavePostDto
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
}
