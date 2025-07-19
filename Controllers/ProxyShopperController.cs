using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using LocalMartOnline.Models.DTOs.Product;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Models.DTOs.ProxyShopping;
using LocalMartOnline.Models.DTOs.Seller;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProxyShopperController : ControllerBase
    {
        private readonly IProxyShopperService _service;
        public ProxyShopperController(IProxyShopperService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] ProxyShopperRegistrationRequestDTO dto)
        {
            var userId = ""; // Lấy userId từ Claims
            await _service.RegisterProxyShopperAsync(dto, userId);
            return Ok(new { success = true });
        }

        [HttpGet("orders")]
        [Authorize]
        public async Task<IActionResult> GetAvailableOrders()
        {
            var orders = await _service.GetAvailableOrdersAsync();
            return Ok(new { success = true, data = orders });
        }

        [HttpPost("orders/{orderId}/accept")]
        [Authorize]
        public async Task<IActionResult> AcceptOrder(string orderId)
        {
            var proxyShopperId = ""; // Lấy userId từ Claims
            await _service.AcceptOrderAsync(orderId, proxyShopperId);
            return Ok(new { success = true });
        }

        [HttpPost("orders/{orderId}/proposal")]
        [Authorize]
        public async Task<IActionResult> SendProposal(string orderId, [FromBody] ProxyShoppingProposalDTO proposal)
        {
            await _service.SendProposalAsync(orderId, proposal);
            return Ok(new { success = true });
        }

        [HttpPost("orders/{orderId}/confirm")]
        [Authorize]
        public async Task<IActionResult> ConfirmOrder(string orderId)
        {
            var proxyShopperId = ""; // Lấy userId từ Claims
            await _service.ConfirmOrderAsync(orderId, proxyShopperId);
            return Ok(new { success = true });
        }

        [HttpPost("orders/{orderId}/upload")]
        [Authorize]
        public async Task<IActionResult> UploadBoughtItems(string orderId, [FromBody] List<string> imageUrls, [FromQuery] string note)
        {
            await _service.UploadBoughtItemsAsync(orderId, imageUrls, note);
            return Ok(new { success = true });
        }

        [HttpPost("orders/{orderId}/final-price")]
        [Authorize]
        public async Task<IActionResult> ConfirmFinalPrice(string orderId, [FromQuery] decimal finalPrice)
        {
            await _service.ConfirmFinalPriceAsync(orderId, finalPrice);
            return Ok(new { success = true });
        }

        [HttpPost("orders/{orderId}/delivery")]
        [Authorize]
        public async Task<IActionResult> ConfirmDelivery(string orderId)
        {
            var result = await _service.ConfirmDeliveryAsync(orderId);
            if (!result)
                return BadRequest(new { success = false, message = "Không thể xác nhận giao hàng hoặc đơn hàng không ở trạng thái phù hợp." });
            
            return Ok(new { success = true, message = "Đã xác nhận giao hàng thành công và cập nhật thống kê sản phẩm." });
        }

        [HttpPut("orders/{orderId}/items/{productId}")]
        [Authorize]
        public async Task<IActionResult> ReplaceOrRemoveProduct(string orderId, string productId, [FromBody] ProductDto? replacementItem)
        {
            var result = await _service.ReplaceOrRemoveProductAsync(orderId, productId, replacementItem);
            return Ok(new { success = result });
        }

        [HttpGet("products/smart-search")]
        [Authorize]
        public async Task<IActionResult> SmartSearchProducts([FromQuery] string q, [FromQuery] int limit = 10)
        {
            var products = await _service.SmartSearchProductsAsync(q, limit);
            return Ok(new { success = true, data = products });
        }

        // Order management endpoints for ProxyShopper
        [HttpGet("my-orders")]
        [Authorize]
        public async Task<IActionResult> GetMyOrders([FromQuery] string? status = null)
        {
            var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(proxyShopperId))
                return Unauthorized(new { success = false, message = "Không xác định được ProxyShopper." });

            var orders = await _service.GetMyOrdersAsync(proxyShopperId, status);
            return Ok(new { success = true, data = orders });
        }

        [HttpGet("orders/{orderId}/detail")]
        [Authorize]
        public async Task<IActionResult> GetOrderDetail(string orderId)
        {
            var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(proxyShopperId))
                return Unauthorized(new { success = false, message = "Không xác định được ProxyShopper." });

            var order = await _service.GetOrderDetailAsync(orderId, proxyShopperId);
            if (order == null)
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng." });

            return Ok(new { success = true, data = order });
        }

        [HttpGet("order-history")]
        [Authorize]
        public async Task<IActionResult> GetOrderHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(proxyShopperId))
                return Unauthorized(new { success = false, message = "Không xác định được ProxyShopper." });

            var orders = await _service.GetOrderHistoryAsync(proxyShopperId, page, pageSize);
            return Ok(new { success = true, data = orders, page, pageSize });
        }

        [HttpPost("orders/{orderId}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelOrder(string orderId, [FromBody] CancelOrderRequestDTO request)
        {
            var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(proxyShopperId))
                return Unauthorized(new { success = false, message = "Không xác định được ProxyShopper." });

            var result = await _service.CancelOrderAsync(orderId, proxyShopperId, request.Reason);
            if (!result)
                return BadRequest(new { success = false, message = "Không thể hủy đơn hàng hoặc đơn hàng không ở trạng thái phù hợp." });

            return Ok(new { success = true, message = "Đã hủy đơn hàng thành công." });
        }

        [HttpGet("my-stats")]
        [Authorize]
        public async Task<IActionResult> GetMyStats()
        {
            var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(proxyShopperId))
                return Unauthorized(new { success = false, message = "Không xác định được ProxyShopper." });

            var stats = await _service.GetMyStatsAsync(proxyShopperId);
            return Ok(new { success = true, data = stats });
        }
    }
}
