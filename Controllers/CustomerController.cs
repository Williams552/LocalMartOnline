using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Models.DTOs.LoyalCustomer;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _loyalCustomerService;

        public CustomerController(ICustomerService loyalCustomerService)
        {
            _loyalCustomerService = loyalCustomerService;
        }

        /// <summary>
        /// Get loyal customers for the seller - Seller only
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLoyalCustomers([FromQuery] GetLoyalCustomersRequestDto request, [FromHeader] string userId = "", [FromHeader] string userRole = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    return BadRequest(new { message = "User ID and role are required" });

                if (userRole != "Seller")
                    return Forbid("Only Sellers can view loyal customers");

                if (request == null)
                    return BadRequest(new { message = "Request parameters are required" });

                // Validate request parameters
                if (request.Page < 1) request.Page = 1;
                if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 20;
                if (request.MinimumOrders < 1) request.MinimumOrders = 5;
                if (request.DaysRange < 1) request.DaysRange = 365;

                var result = await _loyalCustomerService.GetLoyalCustomersAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving loyal customers", error = ex.Message });
            }
        }

        /// <summary>
        /// Get detailed order summary for a specific customer - Seller only
        /// </summary>
        [HttpGet("customer/{customerId}/orders")]
        public async Task<IActionResult> GetCustomerOrderSummary(string customerId, [FromHeader] string userId = "", [FromHeader] string userRole = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    return BadRequest(new { message = "User ID and role are required" });

                if (userRole != "Seller")
                    return Forbid("Only Sellers can view customer order summaries");

                if (string.IsNullOrEmpty(customerId))
                    return BadRequest(new { message = "Customer ID is required" });

                var result = await _loyalCustomerService.GetCustomerOrderSummaryAsync(userId, customerId);
                
                if (result == null)
                    return NotFound(new { message = "Customer not found or no orders with this seller" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving customer order summary", error = ex.Message });
            }
        }

        /// <summary>
        /// Get loyal customer statistics for the seller - Seller only
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetLoyalCustomerStatistics([FromHeader] string userId = "", [FromHeader] string userRole = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    return BadRequest(new { message = "User ID and role are required" });

                if (userRole != "Seller")
                    return Forbid("Only Sellers can view loyal customer statistics");

                var result = await _loyalCustomerService.GetLoyalCustomerStatisticsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving loyal customer statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Get loyalty score calculation for reference - Seller only
        /// </summary>
        [HttpGet("loyalty-score-info")]
        public IActionResult GetLoyaltyScoreInfo([FromHeader] string userRole = "")
        {
            try
            {
                if (userRole != "Seller")
                    return Forbid("Only Sellers can view loyalty score information");

                var info = new
                {
                    description = "Loyalty score calculation methodology",
                    components = new
                    {
                        orderScore = new { description = "10 points per order", maximum = 500 },
                        spendingScore = new { description = "1 point per 10 currency units spent", maximum = 300 },
                        durationBonus = new { description = "5 points per month as customer", maximum = 100 },
                        frequencyScore = new { description = "Based on order frequency", maximum = 100 },
                        recencyPenalty = new { description = "2 points deducted per week of inactivity", minimum = 0 }
                    },
                    tiers = new
                    {
                        bronze = "0-399 points",
                        silver = "400-599 points",
                        gold = "600-799 points",
                        platinum = "800+ points"
                    },
                    loyalCustomerCriteria = new
                    {
                        minimumOrders = "5 completed orders (default)",
                        timeRange = "365 days (default)",
                        customizable = "Sellers can adjust minimum orders and spending thresholds"
                    }
                };

                return Ok(info);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving loyalty score information", error = ex.Message });
            }
        }
    }
}
