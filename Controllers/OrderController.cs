using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Models.DTOs.Order;
using LocalMartOnline.Models.DTOs.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        // UC070: Place Order
        [HttpPost]
        [Authorize(Roles = "Buyer, ProxyShopper")]
        public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] OrderCreateDto dto)
        {
            var result = await _service.PlaceOrderAsync(dto);
            return CreatedAtAction(nameof(GetOrderList), new { buyerId = result.BuyerId }, result);
        }

        // UC071: View Order List
        [HttpGet("buyer/{buyerId}")]
        [Authorize(Roles = "Buyer, ProxyShopper, Seller")]
        public async Task<ActionResult<PagedResultDto<OrderDto>>> GetOrderList(
            string buyerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetOrderListAsync(buyerId, page, pageSize);
            return Ok(result);
        }

        // UC072: Filter Order List
        [HttpPost("filter")]
        [Authorize(Roles = "Buyer, ProxyShopper, Seller")]
        public async Task<ActionResult<PagedResultDto<OrderDto>>> FilterOrderList([FromBody] OrderFilterDto filter)
        {
            var result = await _service.FilterOrderListAsync(filter);
            return Ok(result);
        }

        // Lấy danh sách đơn hàng của seller hiện tại
        [HttpGet("seller/my")]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult<PagedResultDto<OrderDto>>> GetMyOrderList([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Không xác định được seller." });
            var result = await _service.GetOrderListBySellerAsync(userId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("admin/orders")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResultDto<OrderDto>>> GetAllOrderList([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Không xác định được admin." });
            var result = await _service.GetAllOrdersAsync(page, pageSize);
            return Ok(result);
        }

        // Xác thực đơn hàng thành công
        [HttpPost("{orderId}/complete")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<ActionResult> CompleteOrder(string orderId)
        {
            var result = await _service.CompleteOrderAsync(orderId);
            if (!result)
                return BadRequest(new { success = false, message = "Không thể xác thực đơn hàng hoặc đơn hàng đã hoàn thành." });

            return Ok(new { success = true, message = "Đơn hàng đã được xác thực thành công." });
        }

        // ...existing code...

        [HttpPost("place-orders-from-cart")]
        public async Task<IActionResult> PlaceOrdersFromCart([FromBody] CartOrderCreateDto dto)
        {
            try
            {
                if (dto.CartItems == null || !dto.CartItems.Any())
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Giỏ hàng trống"
                    });
                }

                var orders = await _service.PlaceOrdersFromCartAsync(dto);

                return Ok(new
                {
                    Success = true,
                    Message = $"Đã tạo thành công {orders.Count} đơn hàng từ {orders.Count} cửa hàng khác nhau. Thông báo đã được gửi đến các người bán.",
                    Data = new
                    {
                        OrderCount = orders.Count,
                        TotalAmount = orders.Sum(o => o.TotalAmount),
                        Orders = orders,
                        NotificationsSent = orders.Count // Số thông báo đã gửi
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi tạo đơn hàng",
                    Error = ex.Message
                });
            }
        }

        // ...existing code...
    }
}