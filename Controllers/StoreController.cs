using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Models.DTOs.Product;
using LocalMartOnline.Models.DTOs.Store;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        public StoreController(IStoreService storeService, IProductService productService)
        {
            _storeService = storeService;
            _productService = productService;
        }

        // UC030: Open Store
        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult<StoreDto>> OpenStore([FromBody] StoreCreateDto dto)
        {
            var result = await _storeService.CreateStoreAsync(dto);
            return CreatedAtAction(nameof(GetStoreProfile), new { id = result.Id }, result);
        }

        // UC031: Close Store
        [HttpPatch("{id}/close")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CloseStore(string id)
        {
            var result = await _storeService.CloseStoreAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // UC032: Update Store
        [HttpPut("{id}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UpdateStore(string id, [FromBody] StoreUpdateDto dto)
        {
            var result = await _storeService.UpdateStoreAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        // UC037: Follow Store
        [HttpPost("{storeId}/follow")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> FollowStore(long storeId, [FromQuery] long userId)
        {
            var result = await _storeService.FollowStoreAsync(userId, storeId);
            if (!result) return BadRequest("Already following or invalid.");
            return Ok();
        }

        // UC039: Unfollow Store
        [HttpPost("{storeId}/unfollow")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> UnfollowStore(long storeId, [FromQuery] long userId)
        {
            var result = await _storeService.UnfollowStoreAsync(userId, storeId);
            if (!result) return NotFound();
            return Ok();
        }

        // UC038: View Following Store List
        [HttpGet("following")]
        [Authorize(Roles = "Buyer")]
        public async Task<ActionResult<IEnumerable<StoreDto>>> GetFollowingStores([FromQuery] long userId)
        {
            var stores = await _storeService.GetFollowingStoresAsync(userId);
            return Ok(stores);
        }

        // UC040: View Store Profile
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<StoreDto>> GetStoreProfile(string id)
        {
            var store = await _storeService.GetStoreProfileAsync(id);
            if (store == null) return NotFound();
            return Ok(store);
        }

        // Xem tất cả sản phẩm trong store (UC044)
        [HttpGet("{id}/products")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResultDto<ProductDto>>> GetStoreProducts(
          string id,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 20)
        {
            var result = await _productService.GetProductsByStoreAsync(id, page, pageSize);
            return Ok(result);
        }

        [HttpPost("{id}/products/filter")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResultDto<ProductDto>>> FilterStoreProducts(
            string id,
            [FromBody] ProductFilterDto filter)
        {
            filter.StoreId = id;
            var result = await _productService.FilterProductsInStoreAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}/products/search")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResultDto<ProductDto>>> SearchStoreProducts(
            string id,
            [FromQuery] string keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _productService.SearchProductsInStoreAsync(id, keyword, page, pageSize);
            return Ok(result);
        }

        // Xem chi tiết sản phẩm trong store (UC047)
        [HttpGet("{storeId}/products/{productId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductDto>> GetProductDetailsInStore(
            string storeId,
            string productId,
            [FromServices] IProductService productService)
        {
            var product = await productService.GetProductDetailsInStoreAsync(storeId, productId);
            if (product == null) return NotFound();
            return Ok(product);
        }
    }
}