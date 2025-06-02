using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using System.Collections.Generic;
using System.Linq;
using LocalMartOnline.Models.DTOs;

namespace LocalMartOnline.Services
{
    public class NotificationService : INotificationService
    {
        private static bool _isInitialized = false;
        private readonly IRepository<User> _userRepo;

        public NotificationService(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
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

        public async Task<int> SendNotificationByConditionAsync(NotificationConditionRequestDTO request)
        {
            var users = await _userRepo.FindManyAsync(u =>
                (string.IsNullOrEmpty(request.Role) || u.Role == request.Role) &&
                (string.IsNullOrEmpty(request.Status) || u.Status == request.Status)
            );
            int sentCount = 0;
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.UserToken))
                {
                    await SendNotificationAsync(user.UserToken, request.Message);
                    sentCount++;
                }
            }
            return sentCount;
        }

    }
}
