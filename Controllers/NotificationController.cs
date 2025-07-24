using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LocalMartOnline.Services;
using System.Linq;
using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using LocalMartOnline.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("send")]
        [Authorize]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequestDTO request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Invalid payload.", data = (object?)null });
            }
            if (string.IsNullOrWhiteSpace(request.UserToken))
            {
                return BadRequest(new { success = false, message = "UserToken is required.", data = (object?)null });
            }
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { success = false, message = "Message is required.", data = (object?)null });
            }
            try
            {
                await _notificationService.SendNotificationAsync(request.UserToken, request.Message);
                return Ok(new { success = true, message = "Notification sent successfully!", data = (object?)null });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Failed to send notification: {ex.Message}", data = (object?)null });
            }
        }

        [HttpPost("send-by-condition")]
        [Authorize]
        public async Task<IActionResult> SendNotificationByCondition([FromBody] NotificationConditionRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest(new { success = false, message = "Message is required.", data = (object?)null });
            }
            var sentCount = await _notificationService.SendNotificationByConditionAsync(request);
            if (sentCount == 0)
            {
                return NotFound(new { success = false, message = "No matching users to send notification.", data = 0 });
            }
            return Ok(new { success = true, message = $"Notification sent to {sentCount} users.", data = sentCount });
        }

        [HttpPost("store-suspension")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> NotifyStoreSuspension([FromBody] StoreSuspensionNotificationDto dto)
        {
            await _notificationService.SendNotificationAsync(dto.UserToken, dto.Message);
            return Ok(new { success = true, message = "Đã gửi thông báo đình chỉ cửa hàng." });
        }

        [HttpPost("admin-notice")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> SendAdminNotice([FromBody] NotificationRequestDTO dto)
        {
            await _notificationService.SendNotificationAsync(dto.UserToken, dto.Message);
            return Ok(new { success = true, message = "Đã gửi thông báo hành chính." });
        }

        [HttpPost("security-alert")]
        [Authorize]
        public async Task<IActionResult> SendSecurityAlert([FromBody] NotificationRequestDTO dto)
        {
            await _notificationService.SendNotificationAsync(dto.UserToken, dto.Message);
            return Ok(new { success = true, message = "Đã gửi cảnh báo bảo mật." });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetNotificationsPaged([FromQuery] int page = 1, [FromQuery] int limit = 5)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Không xác định được user." });
            var (notifications, total) = await _notificationService.GetNotificationsPagedAsync(userId, page, limit);
            return Ok(new { success = true, data = notifications, total });
        }

        [HttpGet("unread-count")]
        [Authorize]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Không xác định được user." });
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { success = true, data = count });
        }

        [HttpPatch("{notificationId}/mark-as-read")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Không xác định được user." });

            if (string.IsNullOrEmpty(notificationId))
                return BadRequest(new { success = false, message = "NotificationId là bắt buộc." });

            var result = await _notificationService.MarkAsReadAsync(notificationId, userId);
            if (!result)
                return NotFound(new { success = false, message = "Không tìm thấy thông báo hoặc bạn không có quyền truy cập." });

            return Ok(new { success = true, message = "Đã đánh dấu thông báo là đã đọc." });
        }

        [HttpPatch("mark-all-as-read")]
        [Authorize]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Không xác định được user." });

            var markedCount = await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { success = true, message = $"Đã đánh dấu {markedCount} thông báo là đã đọc.", data = markedCount });
        }
    }
}