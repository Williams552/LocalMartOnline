using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProxyShopperController : ControllerBase
    {
        private readonly ProxyShopperService _service;
        public ProxyShopperController(ProxyShopperService service)
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
            await _service.ConfirmDeliveryAsync(orderId);
            return Ok(new { success = true });
        }

        [HttpPut("orders/{orderId}/items/{productId}")]
        [Authorize]
        public async Task<IActionResult> ReplaceOrRemoveProduct(string orderId, string productId, [FromBody] ProductDto? replacementItem)
        {
            var result = await _service.ReplaceOrRemoveProductAsync(orderId, productId, replacementItem);
            return Ok(new { success = result });
        }
    }
}
