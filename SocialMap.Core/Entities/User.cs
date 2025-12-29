namespace SocialMap.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? ProfilePhotoUrl { get; set; }
    public string? Bio { get; set; }
    
    /// <summary>
    /// Admin yetkisi
    /// </summary>
    public bool IsAdmin { get; set; } = false;
    
    /// <summary>
    /// Kullanıcı yasaklandı mı
    /// </summary>
    public bool IsBanned { get; set; } = false;
    public string? BanReason { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>(); // Takip edenler
    public ICollection<Follow> Following { get; set; } = new List<Follow>(); // Takip edilenler
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

