using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LocalMartOnline.Services;

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
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        {
            if (string.IsNullOrEmpty(request.UserToken) || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("UserToken và Message là bắt buộc.");
            }
            await _notificationService.SendNotificationAsync(request.UserToken, request.Message);
            return Ok("Notification sent!");
        }
    }

    public class NotificationRequest
    {
        public string UserToken { get; set; }
        public string Message { get; set; }
    }
}
