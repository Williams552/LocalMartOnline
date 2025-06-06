using LocalMartOnline.Models.DTOs.Category;
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
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // UC056: View Category List
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResultDto<CategoryDto>>> GetAllPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _categoryService.GetAllPagedAsync(page, pageSize);
            return Ok(result);
        }

        // UC057: Add Category
        [HttpPost]
        [Authorize (Roles = "Admin")]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryCreateDto dto)
        {
            var created = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // UC059: Edit Category
        [HttpPut("{id}")]
        [Authorize (Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] CategoryUpdateDto dto)
        {
            var result = await _categoryService.UpdateAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        // UC058: Toggle Category (Enable/Disable)
        [HttpPatch("{id}/toggle")]
        [Authorize  (Roles = "Admin")]
        public async Task<IActionResult> Toggle(string id)
        {
            var result = await _categoryService.ToggleAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // UC060: Search Category by name
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> Search([FromQuery] string name)
        {
            var categories = await _categoryService.SearchByNameAsync(name);
            return Ok(categories);
        }

        // UC061: Filter Category by alphabet
        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> Filter([FromQuery] char alphabet)
        {
            var categories = await _categoryService.FilterByAlphabetAsync(alphabet);
            return Ok(categories);
        }

        // Get category by id (for detail or CreatedAtAction)
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryDto>> GetById(string id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        // Optional: Delete category
        [HttpDelete("{id}")]
        [Authorize (Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}