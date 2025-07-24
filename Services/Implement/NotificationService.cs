using System.Threading.Tasks;
using FirebaseAdmin;
using FcmNotification = FirebaseAdmin.Messaging.Notification;
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
        private readonly IRepository<LocalMartOnline.Models.Notification> _notificationRepo;
        private readonly string _firebaseCredentialPath;

        public NotificationService(IRepository<User> userRepo, IRepository<LocalMartOnline.Models.Notification> notificationRepo, IConfiguration configuration)
        {
            _userRepo = userRepo;
            _notificationRepo = notificationRepo;
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
                Notification = new FcmNotification
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


        public Task<IEnumerable<NotificationDto>> GetNotificationsAsync(string userId)
        {
            // Giả lập lấy danh sách notification từ DB, cần thay bằng truy vấn thực tế
            return Task.FromResult<IEnumerable<NotificationDto>>(new List<NotificationDto>());
        }

        public async Task<(IEnumerable<NotificationDto> Notifications, int Total)> GetNotificationsPagedAsync(string userId, int page, int limit)
        {
            // Lấy tất cả notification của userId
            var all = await _notificationRepo.FindManyAsync(n => n.UserId == userId);
            var total = all.Count();
            var paged = all.OrderByDescending(n => n.CreatedAt)
                           .Skip((page - 1) * limit)
                           .Take(limit)
                           .Select(n => new NotificationDto
                           {
                               Id = n.Id,
                               UserId = n.UserId,
                               Title = n.Title,
                               Message = n.Message,
                               Type = n.Type,
                               IsRead = n.IsRead,
                               CreatedAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                           });
            return (paged, total);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            // Đếm số lượng notification chưa đọc của userId
            var unread = await _notificationRepo.FindManyAsync(n => n.UserId == userId && !n.IsRead);
            return unread.Count();
        }

        public async Task<bool> MarkAsReadAsync(string userId, string notificationId)
        {
            try
            {
                // Tìm notification theo id và userId để đảm bảo user chỉ có thể đánh dấu notification của mình
                var notification = await _notificationRepo.FindOneAsync(n => n.Id == notificationId && n.UserId == userId);
                
                if (notification == null)
                {
                    return false; // Không tìm thấy notification hoặc không thuộc về user này
                }

                // Đánh dấu đã đọc
                notification.IsRead = true;
                await _notificationRepo.UpdateAsync(notification.Id, notification);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to mark notification as read: {ex.Message}");
                return false;
            }
        }

        public async Task<int> MarkAllAsReadAsync(string userId)
        {
            try
            {
                // Lấy tất cả notification chưa đọc của user
                var unreadNotifications = await _notificationRepo.FindManyAsync(n => n.UserId == userId && !n.IsRead);
                
                int count = 0;
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    await _notificationRepo.UpdateAsync(notification.Id, notification);
                    count++;
                }
                
                return count;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to mark all notifications as read: {ex.Message}");
                return 0;
            }
        }

    }
}
