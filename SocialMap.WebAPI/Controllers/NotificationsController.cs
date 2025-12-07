using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.Interfaces;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetNotifications(Guid userId)
    {
        var notifications = await _notificationService.GetNotificationsAsync(userId);
        var notificationDtos = notifications.Select(n => new
        {
            n.Id,
            n.Type,
            n.Message,
            n.RelatedPostId,
            n.RelatedUserId,
            n.IsRead,
            n.CreatedAt
        });
        return Ok(notificationDtos);
    }

    [HttpGet("{userId}/unread")]
    public async Task<ActionResult<IEnumerable<object>>> GetUnreadNotifications(Guid userId)
    {
        var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
        var notificationDtos = notifications.Select(n => new
        {
            n.Id,
            n.Type,
            n.Message,
            n.RelatedPostId,
            n.RelatedUserId,
            n.IsRead,
            n.CreatedAt
        });
        return Ok(notificationDtos);
    }

    [HttpGet("{userId}/unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(Guid userId)
    {
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(count);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    [HttpPut("{userId}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(Guid userId)
    {
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }
}

