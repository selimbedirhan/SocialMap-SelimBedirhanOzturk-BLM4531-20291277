using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SocialMap.Core.DTOs;
using SocialMap.Core.Interfaces;
using SocialMap.WebAPI.Services;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FollowsController : ControllerBase
{
    private readonly IFollowService _followService;
    private readonly NotificationBroadcaster _notificationBroadcaster;
    private readonly INotificationRepository _notificationRepository;

    public FollowsController(IFollowService followService, NotificationBroadcaster notificationBroadcaster, INotificationRepository notificationRepository)
    {
        _followService = followService;
        _notificationBroadcaster = notificationBroadcaster;
        _notificationRepository = notificationRepository;
    }

    [HttpPost("{followerId}/follow/{followingId}")]
    public async Task<IActionResult> FollowUser(Guid followerId, Guid followingId)
    {
        try
        {
            await _followService.FollowUserAsync(followerId, followingId);
            
            // Son oluşturulan bildirimi al ve SignalR ile gönder
            var notifications = await _notificationRepository.GetByUserIdAsync(followingId);
            var lastNotification = notifications.OrderByDescending(n => n.CreatedAt).FirstOrDefault();
            if (lastNotification != null)
            {
                await _notificationBroadcaster.BroadcastNotificationAsync(lastNotification);
            }
            
            return Ok(new { message = "Kullanıcı takip edildi." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{followerId}/unfollow/{followingId}")]
    public async Task<IActionResult> UnfollowUser(Guid followerId, Guid followingId)
    {
        try
        {
            await _followService.UnfollowUserAsync(followerId, followingId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{followerId}/is-following/{followingId}")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> IsFollowing(Guid followerId, Guid followingId)
    {
        var isFollowing = await _followService.IsFollowingAsync(followerId, followingId);
        return Ok(isFollowing);
    }

    [HttpGet("{userId}/followers")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetFollowers(Guid userId)
    {
        var followers = await _followService.GetFollowersAsync(userId);
        var followerDtos = followers.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            ProfilePhotoUrl = u.ProfilePhotoUrl,
            Bio = u.Bio,
            CreatedAt = u.CreatedAt
        });
        return Ok(followerDtos);
    }

    [HttpGet("{userId}/following")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetFollowing(Guid userId)
    {
        var following = await _followService.GetFollowingAsync(userId);
        var followingDtos = following.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            ProfilePhotoUrl = u.ProfilePhotoUrl,
            Bio = u.Bio,
            CreatedAt = u.CreatedAt
        });
        return Ok(followingDtos);
    }

    [HttpGet("{userId}/stats")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetStats(Guid userId)
    {
        var followerCount = await _followService.GetFollowerCountAsync(userId);
        var followingCount = await _followService.GetFollowingCountAsync(userId);
        return Ok(new { followerCount, followingCount });
    }
}

