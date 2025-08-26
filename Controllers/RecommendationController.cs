// Controllers/RecommendationController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LocalMartOnline.Services;
using System.Security.Claims;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly IAIRecommendationService _aiService;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(IAIRecommendationService aiService, ILogger<RecommendationController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        /// <summary>
        /// Get personalized recommendations for current authenticated user (using JWT token)
        /// </summary>
        /// <param name="count">Number of recommendations (max 20)</param>
        /// <returns>List of recommended products for the authenticated user</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<List<ProductRecommendationDto>>> GetMyRecommendations(
            [FromQuery] int count = 5)
        {
            try
            {
                // Get user ID from JWT token claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "User ID not found in token. Please login again.",
                        data = (object?)null
                    });
                }

                if (count <= 0 || count > 20)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Count must be between 1 and 20",
                        data = (object?)null
                    });
                }

                _logger.LogInformation("Getting recommendations for authenticated user {UserId}, count: {Count}", userId, count);

                var recommendations = await _aiService.GetRecommendationsAsync(userId, count);

                return Ok(new
                {
                    success = true,
                    data = recommendations,
                    count = recommendations.Count,
                    userId = userId,
                    message = "Recommendations for authenticated user",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
                _logger.LogError(ex, "Error getting recommendations for authenticated user {UserId}", userId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error getting recommendations for authenticated user",
                    error = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Get personalized recommendations for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="count">Number of recommendations (max 20)</param>
        /// <returns>List of recommended products</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ProductRecommendationDto>>> GetUserRecommendations(
            string userId, 
            [FromQuery] int count = 5)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID is required");
                }

                if (count <= 0 || count > 20)
                {
                    return BadRequest("Count must be between 1 and 20");
                }

                _logger.LogInformation("Getting recommendations for user {UserId}, count: {Count}", userId, count);

                var recommendations = await _aiService.GetRecommendationsAsync(userId, count);

                return Ok(new
                {
                    success = true,
                    data = recommendations,
                    count = recommendations.Count,
                    userId = userId,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations for user {UserId}", userId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error getting recommendations",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Trigger AI model retraining
        /// </summary>
        /// <returns>Retraining status</returns>
        [HttpPost("retrain")]
        public async Task<ActionResult> TriggerModelRetraining()
        {
            try
            {
                _logger.LogInformation("Triggering AI model retraining");

                var success = await _aiService.TriggerRetrainingAsync();

                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Model retraining started in background",
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Failed to trigger model retraining"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering model retraining");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error triggering model retraining",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get AI service status and health information
        /// </summary>
        /// <returns>AI service status</returns>
        [HttpGet("status")]
        public async Task<ActionResult> GetAIServiceStatus()
        {
            try
            {
                var status = await _aiService.GetAIStatusAsync();
                var isHealthy = await _aiService.IsHealthyAsync();

                return Ok(new
                {
                    success = true,
                    healthy = isHealthy,
                    aiService = status,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI service status");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error getting AI service status",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get popular products (fallback when AI is unavailable)
        /// </summary>
        /// <param name="count">Number of products</param>
        /// <returns>Popular products</returns>
        [HttpGet("popular")]
        public ActionResult GetPopularProducts([FromQuery] int count = 10)
        {
            try
            {
                // Use the AI service fallback method to fetch popular products
                var popular = _aiService.GetFallbackRecommendationsAsync(count).GetAwaiter().GetResult();

                return Ok(new
                {
                    success = true,
                    data = popular,
                    count = popular?.Count ?? 0,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular products");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error getting popular products",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>Service health status</returns>
        [HttpGet("health")]
        public async Task<ActionResult> HealthCheck()
        {
            try
            {
                var aiHealthy = await _aiService.IsHealthyAsync();
                
                return Ok(new
                {
                    service = "RecommendationController",
                    healthy = true,
                    aiServiceHealthy = aiHealthy,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new
                {
                    service = "RecommendationController", 
                    healthy = false,
                    error = ex.Message
                });
            }
        }
    }
}
