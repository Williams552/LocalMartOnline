using Microsoft.AspNetCore.Mvc;
<<<<<<< HEAD
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
=======
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Models.DTOs.Product;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocalMartOnline.Models.DTOs.Common;
using Microsoft.AspNetCore.Authorization;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            _service = service;
        }

        // UC041: Add Product
        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult<ProductDto>> AddProduct([FromBody] ProductCreateDto dto)
        {
            var result = await _service.AddProductAsync(dto);
            return CreatedAtAction(nameof(GetProductDetails), new { id = result.Id }, result);
        }

        // UC042: Edit Product
        [HttpPut("{id}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> EditProduct(string id, [FromBody] ProductUpdateDto dto)
        {
            var result = await _service.EditProductAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        // UC043: Toggle Product
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> ToggleProduct(string id, [FromQuery] bool enable)
        {
            var result = await _service.ToggleProductAsync(id, enable);
            if (!result) return NotFound();
            return NoContent();
        }

        // UC049: View All Product List (phân trang)
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResultDto<ProductDto>>> GetAllProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var products = await _service.GetAllProductsAsync(page, pageSize);
            return Ok(products);
        }

        // UC050: View Product Details
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductDto>> GetProductDetails(string id)
        {
            var product = await _service.GetProductDetailsAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // UC053: Upload Actual Product Photo
        [HttpPost("{productId}/actual-photo")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UploadActualPhoto(string productId, [FromBody] ProductActualPhotoUploadDto dto)
        {
            dto.ProductId = productId;
            var result = await _service.UploadActualProductPhotoAsync(dto);
            if (!result) return NotFound();
            return Ok();
        }

        // UC054: Search Products (phân trang)
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResultDto<ProductDto>>> Search(
            [FromQuery] string keyword,
            [FromQuery] string? categoryId,
            [FromQuery] decimal? latitude,
            [FromQuery] decimal? longitude,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var products = await _service.SearchProductsAsync(keyword, categoryId, latitude, longitude, page, pageSize);
            return Ok(products);
        }

        // UC055: Filter Products (phân trang)
        [HttpPost("filter")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResultDto<ProductDto>>> Filter([FromBody] ProductFilterDto filter)
        {
            var products = await _service.FilterProductsAsync(filter);
            return Ok(products);
>>>>>>> origin/develop
        }
    }
}