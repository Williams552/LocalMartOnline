using LocalMartOnline.Models.DTOs.CategoryRegistration;
using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryRegistrationController : ControllerBase
    {
        private readonly ICategoryRegistrationService _service;

        public CategoryRegistrationController(ICategoryRegistrationService service)
        {
            _service = service;
        }

        // UC062: Register Category
        [HttpPost]
        [Authorize (Roles = "Seller")]
        public async Task<ActionResult<CategoryRegistrationDto>> Register([FromBody] CategoryRegistrationCreateDto dto)
        {
            var result = await _service.RegisterAsync(dto);
            return CreatedAtAction(nameof(GetAllPaged), new { id = result.Id }, result);
        }

        // UC063: View Category Registration List
        [HttpGet]
        [Authorize (Roles = "Admin, MS, LGR, MMBH")]
        public async Task<ActionResult<PagedResultDto<CategoryRegistrationDto>>> GetAllPaged(
         [FromQuery] int page = 1,
         [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetAllPagedAsync(page, pageSize);
            return Ok(result);
        }

        // UC064: Approve Category Registration
        [HttpPost("{id}/approve")]
        [Authorize (Roles = "Admin, MS")]
        public async Task<IActionResult> Approve(string id)
        {
            var result = await _service.ApproveAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // UC065: Reject Category Registration
        [HttpPost("{id}/reject")]
        [Authorize (Roles = "Admin, MS")]
        public async Task<IActionResult> Reject(string id, [FromBody] CategoryRegistrationRejectDto dto)
        {
            var result = await _service.RejectAsync(id, dto.RejectionReason);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}