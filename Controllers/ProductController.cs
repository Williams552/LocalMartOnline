using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using LocalMartOnline.Services;
using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // ...existing code...

        [HttpGet("search")]
        public async Task<ActionResult<SearchProductResultDto>> SearchProducts([FromQuery] SearchProductRequestDto request)
        {
            try
            {
                var result = await _productService.SearchProductsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching products", error = ex.Message });
            }
        }
    }
}