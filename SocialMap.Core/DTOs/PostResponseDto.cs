namespace SocialMap.Core.DTOs;

public class PostResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public Guid? PlaceId { get; set; } // Nullable for backward compatibility
    public string PlaceName { get; set; } = null!;
    public string PlaceLocation { get; set; } = null!; // "Anıtkabir - Ankara - Türkiye"
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? MediaUrl { get; set; }
    public string? Caption { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CommentResponseDto> Comments { get; set; } = new();
    public List<LikeResponseDto> Likes { get; set; } = new();
}

