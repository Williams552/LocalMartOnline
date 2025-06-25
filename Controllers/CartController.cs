using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Services;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCartItems(string userId)
        {
            try
            {
                var cartItems = await _cartService.GetCartItemsAsync(userId);
                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving cart items", error = ex.Message });
            }
        }

        [HttpPost("{userId}/items")]
        public async Task<IActionResult> AddToCart(string userId, [FromBody] AddToCartRequest request)
        {
            try
            {
                var success = await _cartService.AddToCartAsync(userId, request.ProductId, request.Quantity);
                
                if (!success)
                    return BadRequest(new { message = "Insufficient stock or invalid product" });
                
                return Ok(new { message = "Product added to cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding to cart", error = ex.Message });
            }
        }

        [HttpPut("{userId}/items/{productId}")]
        public async Task<IActionResult> UpdateCartItem(string userId, string productId, [FromBody] UpdateCartItemRequest request)
        {
            try
            {
                var success = await _cartService.UpdateCartItemAsync(userId, productId, request.Quantity);
                
                if (!success)
                    return BadRequest(new { message = "Insufficient stock or invalid request" });
                
                return Ok(new { message = "Cart item updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating cart item", error = ex.Message });
            }
        }

        [HttpDelete("{userId}/items/{productId}")]
        public async Task<IActionResult> RemoveFromCart(string userId, string productId)
        {
            try
            {
                var success = await _cartService.RemoveFromCartAsync(userId, productId);
                
                if (!success)
                    return NotFound(new { message = "Cart item not found" });
                
                return Ok(new { message = "Product removed from cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error removing from cart", error = ex.Message });
            }
        }
    }

    public class AddToCartRequest
    {
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public int Quantity { get; set; }
    }
}