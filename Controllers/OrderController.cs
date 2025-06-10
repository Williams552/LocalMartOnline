using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Models.DTOs.Order;
using LocalMartOnline.Models.DTOs.Common;
using System.Threading.Tasks;

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
        public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] OrderCreateDto dto)
        {
            var result = await _service.PlaceOrderAsync(dto);
            return CreatedAtAction(nameof(GetOrderList), new { buyerId = result.BuyerId }, result);
        }

        // UC071: View Order List
        [HttpGet("buyer/{buyerId}")]
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
        public async Task<ActionResult<PagedResultDto<OrderDto>>> FilterOrderList([FromBody] OrderFilterDto filter)
        {
            var result = await _service.FilterOrderListAsync(filter);
            return Ok(result);
        }
    }
}