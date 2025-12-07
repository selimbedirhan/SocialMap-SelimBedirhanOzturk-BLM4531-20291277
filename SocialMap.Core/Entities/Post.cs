namespace SocialMap.Core.Entities;

public class Post
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? PlaceId { get; set; } // Nullable for backward compatibility
    public Place? Place { get; set; } // Nullable for backward compatibility

    // Instagram benzeri yer etiketleme için direkt yer bilgileri
    public string? PlaceName { get; set; } // Yer adı
    public string? City { get; set; } // Şehir
    public string? Country { get; set; } // Ülke

    public string? MediaUrl { get; set; } // Fotoğraf veya video
    public string? Caption { get; set; }
    public double? Latitude { get; set; } // Post'un konumu
    public double? Longitude { get; set; }
    public string? Geohash { get; set; } // Latitude/Longitude'den hesaplanan geohash
    public int LikesCount { get; set; } = 0;
    public int CommentsCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
}

