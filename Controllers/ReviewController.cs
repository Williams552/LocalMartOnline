using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Services;
using LocalMartOnline.Models.DTOs.Review;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Buyer hoặc ProxyShopper đánh giá số sao sau khi đã hoàn thành đơn
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromQuery] string userId, [FromBody] CreateReviewDto createReviewDto)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { message = "User ID is required" });

                var review = await _reviewService.CreateReviewAsync(userId, createReviewDto);
                
                if (review == null)
                    return BadRequest(new { message = "Unable to create review. Check if you have completed an order or already reviewed this target." });

                return Ok(new { message = "Review created successfully", review });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating review", error = ex.Message });
            }
        }

        /// <summary>
        /// Buyer hoặc ProxyShopper cập nhật lại review
        /// </summary>
        [HttpPut("{reviewId}")]
        public async Task<IActionResult> UpdateReview(string reviewId, [FromQuery] string userId, [FromBody] UpdateReviewDto updateReviewDto)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { message = "User ID is required" });

                var review = await _reviewService.UpdateReviewAsync(userId, reviewId, updateReviewDto);
                
                if (review == null)
                    return NotFound(new { message = "Review not found or you don't have permission to update it" });

                return Ok(new { message = "Review updated successfully", review });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating review", error = ex.Message });
            }
        }

        /// <summary>
        /// Buyer hoặc ProxyShopper xóa review
        /// </summary>
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(string reviewId, [FromQuery] string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { message = "User ID is required" });

                var success = await _reviewService.DeleteReviewAsync(userId, reviewId);
                
                if (!success)
                    return NotFound(new { message = "Review not found or you don't have permission to delete it" });

                return Ok(new { message = "Review deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting review", error = ex.Message });
            }
        }

        /// <summary>
        /// Các người dùng có thể xem được đánh giá của họ (Buyer hoặc ProxyShopper)
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReviewsByUser(string userId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByUserAsync(userId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user reviews", error = ex.Message });
            }
        }

        /// <summary>
        /// Xem đánh giá cho một target cụ thể (Product, Seller, ProxyShopper) với phân trang và lọc
        /// </summary>
        [HttpGet("target")]
        public async Task<IActionResult> GetReviewsForTarget(
            [FromQuery] string targetType, 
            [FromQuery] string targetId,
            [FromQuery] int? rating = null,
            [FromQuery] bool? hasImages = null,
            [FromQuery] bool? verifiedPurchaseOnly = null,
            [FromQuery] string sortBy = "newest",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(targetType) || string.IsNullOrEmpty(targetId))
                    return BadRequest(new { message = "Target type and target ID are required" });

                var filterOptions = new
                {
                    TargetType = targetType,
                    TargetId = targetId,
                    Rating = rating,
                    HasImages = hasImages,
                    VerifiedPurchaseOnly = verifiedPurchaseOnly,
                    SortBy = sortBy,
                    Page = page,
                    PageSize = Math.Min(pageSize, 50) // Limit max page size
                };

                var reviews = await _reviewService.GetReviewsForTargetAsync(filterOptions);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving target reviews", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy một review cụ thể theo ID
        /// </summary>
        [HttpGet("{reviewId}")]
        public async Task<IActionResult> GetReviewById(string reviewId)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(reviewId);
                
                if (review == null)
                    return NotFound(new { message = "Review not found" });

                return Ok(review);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving review", error = ex.Message });
            }
        }
    }
}
