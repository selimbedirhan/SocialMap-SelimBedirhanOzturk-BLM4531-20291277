using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.DTOs;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;

namespace SocialMap.WebAPI.Controllers;

/// <summary>
/// Kullanıcıların içerik raporlaması için API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportRepository _reportRepository;
    private readonly IUserRepository _userRepository;

    public ReportsController(IReportRepository reportRepository, IUserRepository userRepository)
    {
        _reportRepository = reportRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Yeni rapor oluştur
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ReportResponseDto>> CreateReport([FromBody] CreateReportDto dto)
    {
        // Geçerli hedef türleri kontrol et
        var validTargetTypes = new[] { "post", "comment", "user" };
        if (!validTargetTypes.Contains(dto.TargetType.ToLower()))
        {
            return BadRequest("Geçersiz hedef türü. 'post', 'comment' veya 'user' olmalı.");
        }

        // Geçerli rapor nedenleri kontrol et
        var validReasons = new[] { "spam", "harassment", "inappropriate", "hate_speech", "other" };
        if (!validReasons.Contains(dto.Reason.ToLower()))
        {
            return BadRequest("Geçersiz rapor nedeni.");
        }

        var reporter = await _userRepository.GetByIdAsync(dto.ReporterId);
        if (reporter == null)
            return BadRequest("Geçersiz kullanıcı ID");

        var report = new Report
        {
            Id = Guid.NewGuid(),
            ReporterId = dto.ReporterId,
            TargetType = dto.TargetType.ToLower(),
            TargetId = dto.TargetId,
            Reason = dto.Reason.ToLower(),
            Description = dto.Description,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        await _reportRepository.AddAsync(report);

        return CreatedAtAction(nameof(GetReportById), new { id = report.Id }, new ReportResponseDto
        {
            Id = report.Id,
            ReporterId = report.ReporterId,
            ReporterUsername = reporter.Username,
            TargetType = report.TargetType,
            TargetId = report.TargetId,
            Reason = report.Reason,
            Description = report.Description,
            Status = report.Status,
            CreatedAt = report.CreatedAt
        });
    }

    /// <summary>
    /// Rapor detayını getir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ReportResponseDto>> GetReportById(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
            return NotFound();

        var reporter = await _userRepository.GetByIdAsync(report.ReporterId);

        return Ok(new ReportResponseDto
        {
            Id = report.Id,
            ReporterId = report.ReporterId,
            ReporterUsername = reporter?.Username,
            TargetType = report.TargetType,
            TargetId = report.TargetId,
            Reason = report.Reason,
            Description = report.Description,
            Status = report.Status,
            AdminNotes = report.AdminNotes,
            ReviewedByUserId = report.ReviewedByUserId,
            CreatedAt = report.CreatedAt,
            ReviewedAt = report.ReviewedAt
        });
    }

    /// <summary>
    /// Kullanıcının kendi raporlarını getir
    /// </summary>
    [HttpGet("my/{userId}")]
    public async Task<ActionResult<IEnumerable<ReportResponseDto>>> GetMyReports(Guid userId)
    {
        var reports = await _reportRepository.GetReportsByReporterAsync(userId);

        var reportDtos = reports.Select(r => new ReportResponseDto
        {
            Id = r.Id,
            ReporterId = r.ReporterId,
            TargetType = r.TargetType,
            TargetId = r.TargetId,
            Reason = r.Reason,
            Description = r.Description,
            Status = r.Status,
            CreatedAt = r.CreatedAt
        });

        return Ok(reportDtos);
    }
}
