using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.DTOs;
using SocialMap.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        var userDtos = users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            ProfilePhotoUrl = u.ProfilePhotoUrl,
            Bio = u.Bio,
            CreatedAt = u.CreatedAt
        });
        return Ok(userDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        var userDto = new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt
        };
        return Ok(userDto);
    }

    [HttpGet("username/{username}")]
    public async Task<ActionResult<UserResponseDto>> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
            return NotFound();

        var userDto = new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt
        };
        return Ok(userDto);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            // Basit hash - production'da BCrypt veya Argon2 kullanılmalı
            var passwordHash = HashPassword(dto.Password);
            var user = await _userService.CreateUserAsync(dto.Username, dto.Email, passwordHash);

            var userDto = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, userDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserResponseDto dto)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            user.Username = dto.Username;
            user.Email = dto.Email;
            user.ProfilePhotoUrl = dto.ProfilePhotoUrl;
            user.Bio = dto.Bio;

            await _userService.UpdateUserAsync(user);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> SearchUsers([FromQuery] string searchTerm)
    {
        var users = await _userService.SearchUsersAsync(searchTerm);
        var userDtos = users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            ProfilePhotoUrl = u.ProfilePhotoUrl,
            Bio = u.Bio,
            CreatedAt = u.CreatedAt
        });
        return Ok(userDtos);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

