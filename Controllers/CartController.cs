using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Services;
using LocalMartOnline.Models.DTOs.Cart;

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
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { success = false, message = "User ID is required" });

                var cartItems = await _cartService.GetCartItemsWithDetailsAsync(userId);
                
                return Ok(new { 
                    success = true, 
                    message = "Cart items retrieved successfully",
                    data = cartItems
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Error retrieving cart items", 
                    error = ex.Message 
                });
            }
        }

        [HttpPost("{userId}/items")]
        public async Task<IActionResult> AddToCart(string userId, [FromBody] AddToCartDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { success = false, message = "User ID is required" });

                // Validate request
                if (request.Quantity <= 0)
                    return BadRequest(new { success = false, message = "Quantity must be greater than 0" });

                var success = await _cartService.AddToCartAsync(userId, request.ProductId, request.Quantity);
                
                if (!success)
                    return BadRequest(new { success = false, message = "Unable to add product to cart. Check product availability and stock." });
                
                return Ok(new { 
                    success = true, 
                    message = "Product added to cart successfully",
                    data = new { productId = request.ProductId, quantity = request.Quantity }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Error adding to cart", 
                    error = ex.Message 
                });
            }
        }

        [HttpPut("{userId}/items/{productId}")]
        public async Task<IActionResult> UpdateCartItem(string userId, string productId, [FromBody] UpdateCartItemDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(productId))
                    return BadRequest(new { success = false, message = "User ID and Product ID are required" });

                // Validate request
                if (request.Quantity < 0)
                    return BadRequest(new { success = false, message = "Quantity cannot be negative" });

                var success = await _cartService.UpdateCartItemAsync(userId, productId, request.Quantity);
                
                if (!success)
                    return BadRequest(new { success = false, message = "Unable to update cart item. Check product availability and stock." });
                
                return Ok(new { 
                    success = true, 
                    message = "Cart item updated successfully",
                    data = new { productId = productId, quantity = request.Quantity }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Error updating cart item", 
                    error = ex.Message 
                });
            }
        }

        [HttpDelete("{userId}/items/{productId}")]
        public async Task<IActionResult> RemoveFromCart(string userId, string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(productId))
                    return BadRequest(new { success = false, message = "User ID and Product ID are required" });

                var success = await _cartService.RemoveFromCartAsync(userId, productId);
                
                if (!success)
                    return NotFound(new { success = false, message = "Cart item not found" });
                
                return Ok(new { success = true, message = "Product removed from cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Error removing from cart", 
                    error = ex.Message 
                });
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> ClearCart(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { success = false, message = "User ID is required" });

                var success = await _cartService.ClearCartAsync(userId);
                
                if (!success)
                    return NotFound(new { success = false, message = "Cart not found or already empty" });
                
                return Ok(new { success = true, message = "Cart cleared successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Error clearing cart", 
                    error = ex.Message 
                });
            }
        }

        // Get cart summary endpoint
        [HttpGet("{userId}/summary")]
        public async Task<IActionResult> GetCartSummary(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { success = false, message = "User ID is required" });

                var summary = await _cartService.GetCartSummaryAsync(userId);
                
                return Ok(new { 
                    success = true, 
                    message = "Cart summary retrieved successfully",
                    data = summary
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Error retrieving cart summary", 
                    error = ex.Message 
                });
            }
        }
    }
}