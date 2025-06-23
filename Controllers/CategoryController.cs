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
        public async Task<IActionResult> GetAllPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _categoryService.GetAllPagedAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách danh mục thành công",
                data = result
            });
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAdminPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _categoryService.GetAllPagedAdminAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách danh mục quản trị thành công",
                data = result
            });
        }

        // UC057: Add Category
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            var created = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                new
                {
                    success = true,
                    message = "Tạo danh mục mới thành công",
                    data = created
                }
            );
        }

        // UC059: Edit Category
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] CategoryUpdateDto dto)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID không hợp lệ",
                    data = (object?)null
                });
            }

            var result = await _categoryService.UpdateAsync(id, dto);
            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy danh mục để cập nhật",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Cập nhật danh mục thành công",
                data = (object?)null
            });
        }

        // UC058: Toggle Category (Enable/Disable)
        [HttpPatch("{id}/toggle")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Toggle(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID không hợp lệ",
                    data = (object?)null
                });
            }

            var result = await _categoryService.ToggleAsync(id);
            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy danh mục để thay đổi trạng thái",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Thay đổi trạng thái danh mục thành công",
                data = (object?)null
            });
        }

        // UC060: Search Category by name
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            var categories = await _categoryService.SearchByNameAsync(name);
            return Ok(new
            {
                success = true,
                message = "Tìm kiếm danh mục thành công",
                data = categories
            });
        }

        [HttpGet("searchAdmin")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminSearch([FromQuery] string name)
        {
            var categories = await _categoryService.SearchByNameAdmin(name);
            return Ok(new
            {
                success = true,
                message = "Tìm kiếm danh mục thành công",
                data = categories
            });
        }

        // UC061: Filter Category by alphabet
        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<IActionResult> Filter([FromQuery] char alphabet)
        {
            var categories = await _categoryService.FilterByAlphabetAsync(alphabet);
            return Ok(new
            {
                success = true,
                message = $"Lọc danh mục theo chữ cái '{alphabet}' thành công",
                data = categories
            });
        }

        [HttpGet("filterAdmin")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> FilterAdmin([FromQuery] char alphabet)
        {
            var categories = await _categoryService.FilterByAlphabetAdminAsync(alphabet);
            return Ok(new
            {
                success = true,
                message = $"Lọc danh mục theo chữ cái '{alphabet}' thành công",
                data = categories
            });
        }

        // Get category by id (for detail or CreatedAtAction)
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID không hợp lệ, phải là chuỗi hex 24 ký tự",
                    data = (object?)null
                });
            }

            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy danh mục",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Lấy thông tin danh mục thành công",
                data = category
            });
        }

        // Optional: Delete category
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID không hợp lệ",
                    data = (object?)null
                });
            }

            var result = await _categoryService.DeleteAsync(id);
            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy danh mục để xóa",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Xóa danh mục thành công",
                data = (object?)null
            });
        }
    }
}