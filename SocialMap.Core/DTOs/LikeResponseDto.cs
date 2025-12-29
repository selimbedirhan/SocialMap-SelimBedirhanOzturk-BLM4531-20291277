namespace SocialMap.Core.DTOs;

public class LikeResponseDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string? UserProfilePhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

