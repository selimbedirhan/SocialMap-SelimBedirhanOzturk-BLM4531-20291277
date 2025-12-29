namespace SocialMap.Core.DTOs;

public class CreateReportDto
{
    public Guid ReporterId { get; set; }
    public string TargetType { get; set; } = string.Empty; // post, comment, user
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty; // spam, harassment, inappropriate, other
    public string? Description { get; set; }
}

public class ReportResponseDto
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public string? ReporterUsername { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewedByUsername { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class UpdateReportStatusDto
{
    public string Status { get; set; } = string.Empty; // reviewed, resolved, dismissed
    public string? AdminNotes { get; set; }
}

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalPosts { get; set; }
    public int TotalComments { get; set; }
    public int PendingReports { get; set; }
    public int BannedUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int NewPostsToday { get; set; }
}
