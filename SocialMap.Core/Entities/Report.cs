namespace SocialMap.Core.Entities;

/// <summary>
/// İçerik raporlama entity'si
/// Kullanıcıların uygunsuz içerikleri bildirmesi için
/// </summary>
public class Report
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Raporu oluşturan kullanıcı
    /// </summary>
    public Guid ReporterId { get; set; }
    public User? Reporter { get; set; }
    
    /// <summary>
    /// Raporun türü: post, comment, user
    /// </summary>
    public string TargetType { get; set; } = string.Empty;
    
    /// <summary>
    /// Raporlanan içeriğin ID'si
    /// </summary>
    public Guid TargetId { get; set; }
    
    /// <summary>
    /// Rapor nedeni: spam, harassment, inappropriate, other
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Ek açıklama
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Rapor durumu: pending, reviewed, resolved, dismissed
    /// </summary>
    public string Status { get; set; } = "pending";
    
    /// <summary>
    /// Admin tarafından yapılan işlem notu
    /// </summary>
    public string? AdminNotes { get; set; }
    
    /// <summary>
    /// Raporu inceleyen admin
    /// </summary>
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}
