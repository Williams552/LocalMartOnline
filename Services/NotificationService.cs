using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace LocalMartOnline.Services
{
    public class NotificationService : INotificationService
    {
        private static bool _isInitialized = false;

        public NotificationService()
        {
            if (!_isInitialized)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("Key/serviceAccountKey.json")
                });
                _isInitialized = true;
            }
        }

        public async Task SendNotificationAsync(string userToken, string message)
        {
            var notification = new Message()
            {
                Token = userToken, // userToken là FCM token của thiết bị người dùng
                Notification = new Notification
                {
                    Title = "Thông báo từ LocalMartOnline",
                    Body = message
                }
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(notification);
        }
    }
}
