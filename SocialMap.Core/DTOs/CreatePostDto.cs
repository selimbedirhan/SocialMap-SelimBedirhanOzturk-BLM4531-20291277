namespace SocialMap.Core.DTOs;

public class CreatePostDto
{
    public Guid UserId { get; set; }
    public Guid? PlaceId { get; set; } // Nullable for backward compatibility
    public string? PlaceName { get; set; } // Yer adı (Instagram benzeri)
    public double? Latitude { get; set; } // Yer koordinatı
    public double? Longitude { get; set; }
    public string? City { get; set; } // Şehir
    public string? Country { get; set; } // Ülke
    public string? MediaUrl { get; set; }
    public string? Caption { get; set; }
}

