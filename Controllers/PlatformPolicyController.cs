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
        public async Task<ActionResult<IEnumerable<PlatformPolicyDto>>> GetAll([FromQuery] bool? isActive = null)
        {
            var filter = new PlatformPolicyFilterDto { IsActive = isActive };
            var policies = await _policyService.GetAllAsync(filter);
            return Ok(policies);
        }

        // UC104: Create Platform Policy
        [HttpPost]
        [Authorize(Roles = "Admin, LGR")]
        public async Task<ActionResult<PlatformPolicyDto>> Create([FromBody] PlatformPolicyCreateDto dto)
        {
            var createdPolicy = await _policyService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdPolicy.Id }, createdPolicy);
        }

        // UC106: Update Policies
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, LGR")]
        public async Task<IActionResult> Update(string id, [FromBody] PlatformPolicyUpdateDto dto)
        {
            var result = await _policyService.UpdateAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        // UC107: Toggle Platform Policy
        [HttpPut("{id}/toggle")]
        [Authorize(Roles = "Admin, LGR")]
        public async Task<IActionResult> Toggle(string id)
        {
            var result = await _policyService.ToggleAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlatformPolicyDto>> GetById(string id)
        {
            var policy = await _policyService.GetByIdAsync(id);
            if (policy == null) return NotFound();
            return Ok(policy);
        }
    }
}