using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketFeePaymentController : ControllerBase
    {
        private readonly IMarketFeePaymentService _service;
        public MarketFeePaymentController(IMarketFeePaymentService service)
        {
            _service = service;
        }

        [HttpGet("seller/{sellerId}")]
        [Authorize]
        public async Task<IActionResult> GetBySeller(long sellerId)
        {
            var result = await _service.GetPaymentsBySellerAsync(sellerId);
            return Ok(result);
        }

        [HttpGet("{paymentId}")]
        [Authorize]
        public async Task<IActionResult> GetById(long paymentId)
        {
            var result = await _service.GetPaymentByIdAsync(paymentId);
            if (result is null) return NotFound();
            return Ok(result!);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] MarketFeePaymentCreateDto dto)
        {
            var created = await _service.CreatePaymentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { paymentId = created.PaymentId }, created);
        }

        [HttpPatch("{paymentId}/status")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> UpdateStatus(long paymentId, [FromQuery] string status)
        {
            var result = await _service.UpdatePaymentStatusAsync(paymentId, status);
            if (!result) return BadRequest("Cập nhật trạng thái thất bại");
            return Ok("Cập nhật trạng thái thành công");
        }
    }
}
