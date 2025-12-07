namespace SocialMap.Core.DTOs;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? ProfilePhotoUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
}

