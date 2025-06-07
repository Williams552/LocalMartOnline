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
    }
}
