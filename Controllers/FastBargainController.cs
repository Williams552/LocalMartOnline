using LocalMartOnline.Models.DTOs.FastBargain;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
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
    }
}
