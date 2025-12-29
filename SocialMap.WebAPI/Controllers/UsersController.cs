using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.DTOs;
using SocialMap.Core.Interfaces;
using AutoMapper;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public UsersController(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        var userDtos = _mapper.Map<IEnumerable<UserResponseDto>>(users);
        return Ok(userDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        var userDto = _mapper.Map<UserResponseDto>(user);
        return Ok(userDto);
    }

    [HttpGet("username/{username}")]
    public async Task<ActionResult<UserResponseDto>> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
            return NotFound();

        var userDto = _mapper.Map<UserResponseDto>(user);
        return Ok(userDto);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            // UserService artık hashlemeyi kendi yapıyor
            var user = await _userService.CreateUserAsync(dto.Username, dto.Email, dto.Password);

            var userDto = _mapper.Map<UserResponseDto>(user);

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
        var userDtos = _mapper.Map<IEnumerable<UserResponseDto>>(users);
        return Ok(userDtos);
    }


}

