using Microsoft.AspNetCore.SignalR;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.WebAPI.Hubs;

namespace SocialMap.WebAPI.Services;

public class NotificationBroadcaster
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly INotificationRepository _notificationRepository;

    public NotificationBroadcaster(IHubContext<NotificationHub> hubContext, INotificationRepository notificationRepository)
    {
        _hubContext = hubContext;
        _notificationRepository = notificationRepository;
    }

    public async Task BroadcastNotificationAsync(Notification notification)
    {
        await _hubContext.Clients.Group($"user_{notification.UserId}").SendAsync("ReceiveNotification", new
        {
            id = notification.Id,
            userId = notification.UserId,
            type = notification.Type,
            message = notification.Message,
            relatedPostId = notification.RelatedPostId,
            relatedUserId = notification.RelatedUserId,
            isRead = notification.IsRead,
            createdAt = notification.CreatedAt
        });
    }

    public async Task BroadcastRecentNotificationsForUserAsync(Guid userId, int count = 10)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        var recentNotifications = notifications.Take(count);
        
        foreach (var notification in recentNotifications)
        {
            await BroadcastNotificationAsync(notification);
        }
    }
}

