namespace LocalMartOnline.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message);
    }
}
