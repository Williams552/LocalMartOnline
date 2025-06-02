using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LocalMartOnline.Services;
using System.Linq;
using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using LocalMartOnline.Models.DTOs;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IRepository<User> _userRepo;

        public NotificationController(INotificationService notificationService, IUserService userService, IRepository<User> userRepo)
        {
            _notificationService = notificationService;
            _userService = userService;
            _userRepo = userRepo;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        {
            if (string.IsNullOrEmpty(request.UserToken) || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("UserToken và Message là bắt buộc.");
            }
            await _notificationService.SendNotificationAsync(request.UserToken, request.Message);
            return Ok("Notification sent!");
        }

        [HttpPost("send-by-condition")]
        public async Task<IActionResult> SendNotificationByCondition([FromBody] NotificationConditionRequestDTO request)
        {
            var sentCount = await _notificationService.SendNotificationByConditionAsync(request);
            return Ok($"Đã gửi thông báo cho {sentCount} người dùng phù hợp.");
        }
    }

    public class NotificationRequest
    {
        public string? UserToken { get; set; }
        public string? Message { get; set; }
    }

    public class NotificationConditionRequest
    {
        public string? Role { get; set; }
        public string? Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
