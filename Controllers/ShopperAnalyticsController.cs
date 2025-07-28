using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Services.Interface;
using System.Threading.Tasks;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/proxyshopper/analytics")]
    public class ShopperAnalyticsController : ControllerBase
    {
        private readonly IShopperAnalyticsService _service;
        public ShopperAnalyticsController(IShopperAnalyticsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnalytics([FromQuery] string period = "30d")
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Không xác thực được người dùng" });
            var result = await _service.GetShopperAnalyticsAsync(userId, period);
            return Ok(new { success = true, data = result });
        }
    }
}
