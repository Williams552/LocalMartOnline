using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using LocalMartOnline.Services.Interface;
using System;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/seller/analytics")]
    [Authorize(Roles = "Seller")]
    public class SellerAnalyticsController : ControllerBase
    {
        private readonly ISellerAnalyticsService _service;
        public SellerAnalyticsController(ISellerAnalyticsService service)
        {
            _service = service;
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue([FromQuery] string period = "30d")
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _service.GetRevenueAsync(userId, period);
            return Ok(result);
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrderStats([FromQuery] string period = "30d")
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _service.GetOrderStatsAsync(userId, period);
            return Ok(result);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategoryStats([FromQuery] string period = "30d")
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _service.GetCategoryStatsAsync(userId, period);
            return Ok(result);
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProductStats([FromQuery] string period = "30d")
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _service.GetProductStatsAsync(userId, period);
            return Ok(result);
        }
    }
}
