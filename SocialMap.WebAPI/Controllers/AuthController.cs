using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SocialMap.Core.DTOs;
using SocialMap.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public AuthController(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] CreateUserDto dto)
    {
        try
        {
            var passwordHash = HashPassword(dto.Password);
            var user = await _userService.CreateUserAsync(dto.Username, dto.Email, passwordHash);

            var token = GenerateJwtToken(user);
            var userDto = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt
            };

            return Ok(new AuthResponseDto
            {
                Token = token,
                User = userDto
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _userService.ValidateUserAsync(dto.Username, dto.Password);
        if (user == null)
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");

        var token = GenerateJwtToken(user);
        var userDto = new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt
        };

        return Ok(new AuthResponseDto
        {
            Token = token,
            User = userDto
        });
    }

    private string GenerateJwtToken(SocialMap.Core.Entities.User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "SocialMap",
            audience: _configuration["Jwt:Audience"] ?? "SocialMap",
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

