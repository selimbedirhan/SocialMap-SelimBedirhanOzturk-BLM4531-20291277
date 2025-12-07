using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.DTOs;
using SocialMap.Core.Interfaces;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IPostService _postService;
    private readonly IFollowService _followService;

    public ProfilesController(IUserService userService, IPostService postService, IFollowService followService)
    {
        _userService = userService;
        _postService = postService;
        _followService = followService;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<object>> GetProfile(Guid userId)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found" });

            var posts = await _postService.GetPostsByUserIdAsync(userId);
            var followerCount = await _followService.GetFollowerCountAsync(userId);
            var followingCount = await _followService.GetFollowingCountAsync(userId);

            var profileDto = new
            {
                user = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    Bio = user.Bio,
                    CreatedAt = user.CreatedAt
                },
                stats = new
                {
                    postsCount = posts.Count(),
                    followerCount,
                    followingCount
                },
                posts = posts.Select(p => new
                {
                    p.Id,
                    p.MediaUrl,
                    p.Caption,
                    p.LikesCount,
                    commentsCount = p.CommentsCount > 0 ? p.CommentsCount : (p.Comments?.Count ?? 0),
                    p.CreatedAt
                })
            };

            return Ok(profileDto);
        }
        catch (Exception ex)
        {
            // Development'ta detaylı hata göster
            #if DEBUG
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            #else
            return StatusCode(500, new { error = ex.Message });
            #endif
        }
    }
}

