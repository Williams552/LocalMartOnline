using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> AddProduct([FromBody] ProductCreateDto dto)
        {
            var result = await _service.AddProductAsync(dto);
            return CreatedAtAction(nameof(GetProductDetails), new { id = result.Id }, new
            {
                success = true,
                message = "Thêm sản phẩm thành công",
                data = result
            });
        }

        // UC042: Edit Product
        [HttpPut("{id}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> EditProduct(string id, [FromBody] ProductUpdateDto dto)
        {
            var result = await _service.EditProductAsync(id, dto);
            if (!result) return NotFound(new
            {
                success = false,
                message = "Không tìm thấy sản phẩm để cập nhật",
                data = (object?)null
            });
            
            return Ok(new
            {
                success = true,
                message = "Cập nhật sản phẩm thành công",
                data = (object?)null
            });
        }

        // UC043: Toggle Product
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> ToggleProduct(string id, [FromQuery] bool enable)
        {
            var result = await _service.ToggleProductAsync(id, enable);
            if (!result) return NotFound(new
            {
                success = false,
                message = "Không tìm thấy sản phẩm để thay đổi trạng thái",
                data = (object?)null
            });
            
            var status = enable ? "kích hoạt" : "vô hiệu hóa";
            return Ok(new
            {
                success = true,
                message = $"Đã {status} sản phẩm thành công",
                data = (object?)null
            });
        }

        // UC049: View All Product List (phân trang)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var products = await _service.GetAllProductsAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách sản phẩm thành công",
                data = products
            });
        }

        // UC050: View Product Details
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductDetails(string id)
        {
            var product = await _service.GetProductDetailsAsync(id);
            if (product == null) return NotFound(new
            {
                success = false,
                message = "Không tìm thấy thông tin sản phẩm",
                data = (object?)null
            });
            
            return Ok(new
            {
                success = true,
                message = "Lấy thông tin sản phẩm thành công",
                data = product
            });
        }

        // UC053: Upload Actual Product Photo
        [HttpPost("{productId}/actual-photo")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UploadActualPhoto(string productId, [FromBody] ProductActualPhotoUploadDto dto)
        {
            dto.ProductId = productId;
            var result = await _service.UploadActualProductPhotoAsync(dto);
            if (!result) return NotFound(new
            {
                success = false,
                message = "Không tìm thấy sản phẩm để tải ảnh lên",
                data = (object?)null
            });
            
            return Ok(new
            {
                success = true,
                message = "Tải lên ảnh thực tế của sản phẩm thành công",
                data = (object?)null
            });
        }

        // UC054: Search Products (phân trang)
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search(
            [FromQuery] string keyword,
            [FromQuery] string? categoryId,
            [FromQuery] decimal? latitude,
            [FromQuery] decimal? longitude,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var products = await _service.SearchProductsAsync(keyword, categoryId, latitude, longitude, page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Tìm kiếm sản phẩm thành công",
                data = products
            });
        }

        // UC055: Filter Products (phân trang)
        [HttpPost("filter")]
        [AllowAnonymous]
        public async Task<IActionResult> Filter([FromBody] ProductFilterDto filter)
        {
            var products = await _service.FilterProductsAsync(filter);
            return Ok(new
            {
                success = true,
                message = "Lọc sản phẩm thành công",
                data = products
            });
        }
        
        // Get products by store
        [HttpGet("store/{storeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsByStore(
            string storeId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var products = await _service.GetProductsByStoreAsync(storeId, page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách sản phẩm của gian hàng thành công",
                data = products
            });
        }
        
        // Search products in store
        [HttpGet("store/{storeId}/search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchProductsInStore(
            string storeId,
            [FromQuery] string keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var products = await _service.SearchProductsInStoreAsync(storeId, keyword, page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Tìm kiếm sản phẩm trong gian hàng thành công",
                data = products
            });
        }
        
        // Filter products in store
        [HttpPost("store/{storeId}/filter")]
        [AllowAnonymous]
        public async Task<IActionResult> FilterProductsInStore(
            string storeId,
            [FromBody] ProductFilterDto filter)
        {
            filter.StoreId = storeId;
            var products = await _service.FilterProductsInStoreAsync(filter);
            return Ok(new
            {
                success = true,
                message = "Lọc sản phẩm trong gian hàng thành công",
                data = products
            });
        }
        
        //// Get product details in store
        //[HttpGet("store/{storeId}/product/{productId}")]
        //[AllowAnonymous]
        //public async Task<ActionResult<ProductDto>> GetProductDetailsInStore(
        //    string storeId,
        //    string productId)
        //{
        //    var product = await _service.GetProductDetailsInStoreAsync(storeId, productId);
        //    if (product == null) return NotFound();
        //    return Ok(product);
        //}

        // Get all products in a market
        [HttpGet("market/{marketId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsByMarket(
            string marketId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var products = await _service.GetProductsByMarketAsync(marketId, page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách sản phẩm trong chợ thành công",
                data = products
            });
        }

        // Filter products in a market
        [HttpPost("market/{marketId}/filter")]
        [AllowAnonymous]
        public async Task<IActionResult> FilterProductsInMarket(
            string marketId,
            [FromBody] ProductFilterDto filter)
        {
            var products = await _service.FilterProductsInMarketAsync(marketId, filter);
            return Ok(new
            {
                success = true,
                message = "Lọc sản phẩm trong chợ thành công",
                data = products
            });
        }

        // Search products in a market
        [HttpGet("market/{marketId}/search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchProductsInMarket(
            string marketId,
            [FromQuery] string keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var products = await _service.SearchProductsInMarketAsync(marketId, keyword, page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Tìm kiếm sản phẩm trong chợ thành công",
                data = products
            });
        }

        // ============ SELLER ENDPOINTS - INCLUDE ALL PRODUCTS (ACTIVE & INACTIVE) ============
        
        // Get all products for seller (including inactive)
        [HttpGet("seller/store/{storeId}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> GetAllProductsForSeller(
            string storeId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var products = await _service.GetAllProductsForSellerAsync(storeId, page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách sản phẩm của seller thành công",
                data = products
            });
        }

        // Search products for seller (including inactive)
        [HttpGet("seller/store/{storeId}/search")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> SearchProductsForSeller(
            string storeId,
            [FromQuery] string keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var products = await _service.SearchProductsForSellerAsync(storeId, keyword, page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Tìm kiếm sản phẩm của seller thành công",
                data = products
            });
        }

        // Filter products for seller (including inactive)
        [HttpPost("seller/filter")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> FilterProductsForSeller([FromBody] ProductFilterDto filter)
        {
            var products = await _service.FilterProductsForSellerAsync(filter);
            return Ok(new
            {
                success = true,
                message = "Lọc sản phẩm của seller thành công",
                data = products
            });
        }
    }
}