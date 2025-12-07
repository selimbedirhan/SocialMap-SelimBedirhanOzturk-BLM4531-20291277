namespace SocialMap.Core.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public UserResponseDto User { get; set; } = null!;
}

