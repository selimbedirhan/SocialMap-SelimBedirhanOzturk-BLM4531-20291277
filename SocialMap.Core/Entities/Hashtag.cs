namespace SocialMap.Core.Entities;

/// <summary>
/// Hashtag entity
/// Post'larda kullanılan etiketleri takip eder
/// </summary>
public class Hashtag
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Hashtag adı (# olmadan, küçük harf)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Kullanım sayısı (denormalize)
    /// </summary>
    public int UsageCount { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Bu hashtag'i kullanan postlar
    /// </summary>
    public ICollection<PostHashtag> PostHashtags { get; set; } = new List<PostHashtag>();
}

/// <summary>
/// Post-Hashtag ilişki tablosu (Many-to-Many)
/// </summary>
public class PostHashtag
{
    public Guid PostId { get; set; }
    public Post? Post { get; set; }
    
    public Guid HashtagId { get; set; }
    public Hashtag? Hashtag { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
