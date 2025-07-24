using LocalMartOnline.Models.DTOs;

namespace LocalMartOnline.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message);
        Task<int> SendNotificationByConditionAsync(NotificationConditionRequestDTO request);
        Task<IEnumerable<NotificationDto>> GetNotificationsAsync(string userId);
        Task<(IEnumerable<NotificationDto> Notifications, int Total)> GetNotificationsPagedAsync(string userId, int page, int limit);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(string notificationId, string userId);
        Task<int> MarkAllAsReadAsync(string userId);
    }
}
