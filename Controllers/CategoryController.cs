using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Models.DTOs.Category;
using LocalMartOnline.Models.DTOs.Product;
using LocalMartOnline.Services.Interface;

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

        [HttpGet]
        public async Task<ActionResult<GetCategoriesResponseDto>> GetCategories()
        {
            try
            {
                var result = await _categoryService.GetActiveCategoriesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving categories", error = ex.Message });
            }
        }

        [HttpGet("{categoryId}/products")]
        public async Task<ActionResult<SearchProductResultDto>> GetProductsByCategory(string categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string sortPrice = "")
        {
            try
            {
                var result = await _categoryService.GetProductsByCategoryAsync(categoryId, page, pageSize, sortPrice);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving products by category", error = ex.Message });
            }
        }
    }
}