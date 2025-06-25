using LocalMartOnline.Models.DTOs.PlatformPolicy;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlatformPolicyController : ControllerBase
    {
        private readonly IPlatformPolicyService _policyService;

        public PlatformPolicyController(IPlatformPolicyService policyService)
        {
            _policyService = policyService;
        }

        // UC105: View Platform Policies
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PlatformPolicyDto>>> GetAll()
        {
            var policies = await _policyService.GetAllAsync();
            return Ok(policies);
        }

        // UC106: Update Policies
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] PlatformPolicyUpdateDto dto)
        {
            var result = await _policyService.UpdateAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PlatformPolicyDto>> GetById(string id)
        {
            var policy = await _policyService.GetByIdAsync(id);
            if (policy == null) return NotFound();
            return Ok(policy);
        }
    }
}