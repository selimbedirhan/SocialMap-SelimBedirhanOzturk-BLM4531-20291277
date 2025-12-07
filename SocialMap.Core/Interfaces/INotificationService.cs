using SocialMap.Core.Entities;

namespace SocialMap.Core.Interfaces;

public interface INotificationService
{
    Task<Notification> CreateNotificationAsync(Guid userId, string type, string message, Guid? relatedPostId = null, Guid? relatedUserId = null);
    Task<IEnumerable<Notification>> GetNotificationsAsync(Guid userId);
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
}

