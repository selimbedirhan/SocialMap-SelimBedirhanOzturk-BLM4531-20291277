namespace SocialMap.Core.Entities;

/// <summary>
/// Kullanıcının kaydettiği gönderiler (Favoriler/Koleksiyonlar)
/// </summary>
public class SavedPost
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public Guid PostId { get; set; }
    public Post? Post { get; set; }
    
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
