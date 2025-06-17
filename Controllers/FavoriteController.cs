using LocalMartOnline.Models.DTOs.Favorite;
using LocalMartOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Assuming you have authentication
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpPost("add")]
        public async Task<ActionResult<FavoriteActionResponseDto>> AddToFavorite([FromBody] AddToFavoriteRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _favoriteService.AddToFavoriteAsync(userId, request.ProductId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding to favorites", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<GetFavoriteProductsResponseDto>> GetFavoriteProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _favoriteService.GetUserFavoriteProductsAsync(userId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving favorite products", error = ex.Message });
            }
        }

        [HttpDelete("{productId}")]
        public async Task<ActionResult<FavoriteActionResponseDto>> RemoveFromFavorite(string productId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _favoriteService.RemoveFromFavoriteAsync(userId, productId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing from favorites", error = ex.Message });
            }
        }

        [HttpGet("check/{productId}")]
        public async Task<ActionResult<bool>> CheckIfInFavorite(string productId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var isInFavorite = await _favoriteService.IsProductInFavoriteAsync(userId, productId);
                return Ok(new { isInFavorite });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking favorite status", error = ex.Message });
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }
    }
}
