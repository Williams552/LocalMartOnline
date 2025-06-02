using LocalMartOnline.Models.DTOs;

namespace LocalMartOnline.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message);
        Task<int> SendNotificationByConditionAsync(NotificationConditionRequestDTO request);
    }
}
