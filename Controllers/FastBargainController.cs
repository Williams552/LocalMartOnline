using LocalMartOnline.Models.DTOs.FastBargain;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FastBargainController : ControllerBase
    {
        private readonly IFastBargainService _service;
        public FastBargainController(IFastBargainService service)
        {
            _service = service;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartBargain([FromBody] FastBargainCreateRequestDTO request)
        {
            var result = await _service.StartBargainAsync(request);
            return Ok(result);
        }

        [HttpPost("propose")]
        public async Task<IActionResult> Propose([FromBody] FastBargainProposalDTO proposal)
        {
            var result = await _service.ProposeAsync(proposal);
            return Ok(result);
        }

        [HttpPost("action")]
        public async Task<IActionResult> TakeAction([FromBody] FastBargainActionRequestDTO request)
        {
            var result = await _service.TakeActionAsync(request);
            return Ok(result);
        }

        [HttpGet("{bargainId}")]
        public async Task<IActionResult> GetById(string bargainId)
        {
            var result = await _service.GetByIdAsync(bargainId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            var result = await _service.GetByUserIdAsync(userId);
            return Ok(result);
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyBargains()
        {
            // Lấy userId từ JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Không xác định được user." });
            }

            // Lấy role từ JWT token
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userRole))
            {
                return Unauthorized(new { success = false, message = "Không xác định được role của user." });
            }

            var result = await _service.GetMyBargainsAsync(userId, userRole);
            return Ok(new { success = true, data = result, userRole = userRole });
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAllPendingBargains()
        {
            // For admin - get all pending bargains
            var result = await _service.GetAllPendingBargainsAsync();
            return Ok(result);
        }

        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetPendingBargainsBySellerId(string sellerId)
        {
            // For seller - get all pending bargains for specific seller
            var result = await _service.GetPendingBargainsBySellerIdAsync(sellerId);
            return Ok(result);
        }
    }
}
