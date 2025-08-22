using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketFeeController : ControllerBase
    {
        private readonly IMarketFeeService _service;
        public MarketFeeController(IMarketFeeService service)
        {
            _service = service;
        }

        [HttpGet]
        // [Authorize(Roles = "Admin,MarketStaff,Seller")]
        public async Task<IActionResult> GetAll([FromQuery] GetMarketFeeRequestDto request)
        {
            var result = await _service.GetAllAsync(request);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, MS, LGR, MMBH")]
        public async Task<IActionResult> Create([FromBody] MarketFeeCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, MS, LGR, MMBH")]
        public async Task<IActionResult> Update(string id, [FromBody] MarketFeeUpdateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, MS, LGR, MMBH")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPost("pay")]
        [Authorize]
        public async Task<IActionResult> Pay([FromBody] MarketFeePaymentDto dto)
        {
            var result = await _service.PayFeeAsync(dto);
            if (!result) return BadRequest("Thanh toán thất bại");
            return Ok("Thanh toán thành công");
        }
    }
}
