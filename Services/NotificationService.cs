using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using System.Collections.Generic;
using System.Linq;
using LocalMartOnline.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace LocalMartOnline.Services
{
    public class NotificationService : INotificationService
    {
        private static int _isInitialized = 0;
        private static readonly object _initLock = new object();
        private readonly IRepository<User> _userRepo;
        private readonly string _firebaseCredentialPath;

        public NotificationService(IRepository<User> userRepo, IConfiguration configuration)
        {
            _userRepo = userRepo;
            _firebaseCredentialPath = configuration["Firebase:CredentialPath"] ??
                Environment.GetEnvironmentVariable("FIREBASE_CREDENTIAL_PATH") ??
                throw new InvalidOperationException("Firebase credential path not configured");
            if (System.Threading.Interlocked.CompareExchange(ref _isInitialized, 1, 0) == 0)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(_firebaseCredentialPath)
                });
            }
        }

        public async Task SendNotificationAsync(string userToken, string message)
        {
            if (string.IsNullOrWhiteSpace(userToken))
                throw new ArgumentException("userToken must not be null or empty", nameof(userToken));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("message must not be null or empty", nameof(message));

            var notification = new Message()
            {
                Token = userToken, // userToken là FCM token của thiết bị người dùng
                Notification = new Notification
                {
                    Title = "Thông báo từ LocalMartOnline",
                    Body = message
                }
            };

            try
            {
                await FirebaseMessaging.DefaultInstance.SendAsync(notification);
            }
            catch (Exception ex)
            {
                // You can inject ILogger<NotificationService> for better logging
                Console.Error.WriteLine($"Failed to send notification: {ex.Message}");
                throw; // Optionally rethrow or handle as needed
            }
        }

        public async Task<int> SendNotificationByConditionAsync(NotificationConditionRequestDTO request)
        {
            var users = await _userRepo.FindManyAsync(u =>
                (string.IsNullOrEmpty(request.Role) || u.Role == request.Role) &&
                (string.IsNullOrEmpty(request.Status) || u.Status == request.Status)
            );
            var tasks = new List<Task<bool>>();
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.UserToken))
                {
                    tasks.Add(SendNotificationWithHandling(user.UserToken, request.Message));
                }
            }
            var results = await Task.WhenAll(tasks);
            return results.Count(success => success);
        }

        private async Task<bool> SendNotificationWithHandling(string userToken, string message)
        {
            try
            {
                await SendNotificationAsync(userToken, message);
                return true;
            }
            catch (Exception ex)
            {
                // For production, inject ILogger<NotificationService> and use it here
                Console.Error.WriteLine($"Failed to send notification to token {userToken}: {ex.Message}");
                return false;
            }
        }

    }
}
