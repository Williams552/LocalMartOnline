using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketOperationController : ControllerBase
    {
        private readonly IMarketService _marketService;

        public MarketOperationController(IMarketService marketService)
        {
            _marketService = marketService;
        }

        [HttpGet("check-market/{marketId}")]
        public async Task<IActionResult> CheckMarketStatus(string marketId)
        {
            try
            {
                var isOpen = await _marketService.IsMarketOpenAsync(marketId);
                return Ok(new
                {
                    success = true,
                    message = $"Market status checked",
                    data = new
                    {
                        marketId = marketId,
                        isOpen = isOpen,
                        currentTime = DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error checking market status",
                    error = ex.Message
                });
            }
        }

        [HttpPost("update-store-status")]
        public async Task<IActionResult> UpdateStoreStatus()
        {
            try
            {
                await _marketService.UpdateStoreStatusBasedOnMarketHoursAsync();
                return Ok(new
                {
                    success = true,
                    message = "Store statuses updated successfully",
                    data = new
                    {
                        updatedAt = DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error updating store statuses",
                    error = ex.Message
                });
            }
        }

        [HttpGet("test-operating-hours")]
        public IActionResult TestOperatingHours([FromQuery] string operatingHours, [FromQuery] DateTime? testTime = null)
        {
            try
            {
                var currentTime = testTime ?? DateTime.Now;
                var isOpen = _marketService.IsTimeInOperatingHours(operatingHours, currentTime);
                
                return Ok(new
                {
                    success = true,
                    message = "Operating hours test completed",
                    data = new
                    {
                        operatingHours = operatingHours,
                        testTime = currentTime,
                        isOpen = isOpen
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error testing operating hours",
                    error = ex.Message
                });
            }
        }
    }
}
