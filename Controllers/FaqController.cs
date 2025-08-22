using LocalMartOnline.Models.DTOs.Faq;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FaqController : ControllerBase
    {
        private readonly IFaqService _faqService;

        public FaqController(IFaqService faqService)
        {
            _faqService = faqService;
        }

        // UC101: View FAQ
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<FaqDto>>> GetAll()
        {
            var faqs = await _faqService.GetAllAsync();
            return Ok(faqs);
        }

        // UC102: Add FAQ
        [HttpPost]
        [Authorize(Roles = "Admin, LGR")]
        public async Task<ActionResult<FaqDto>> Add([FromBody] FaqCreateDto dto)
        {
            var faq = await _faqService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = faq.Id }, faq);
        }

        // UC103: Update FAQ
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, LGR")]
        public async Task<IActionResult> Update(string id, [FromBody] FaqUpdateDto dto)
        {
            var result = await _faqService.UpdateAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        // UC104: Delete FAQ
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, LGR")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _faqService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FaqDto>> GetById(string id)
        {
            var faq = await _faqService.GetByIdAsync(id);
            if (faq == null) return NotFound();
            return Ok(faq);
        }
    }
}