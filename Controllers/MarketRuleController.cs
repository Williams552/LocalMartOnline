using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Services;
using LocalMartOnline.Models.DTOs.MarketRule;
using Microsoft.AspNetCore.Authorization;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketRuleController : ControllerBase
    {
        private readonly IMarketRuleService _marketRuleService;

        public MarketRuleController(IMarketRuleService marketRuleService)
        {
            _marketRuleService = marketRuleService;
        }

        /// <summary>
        /// Get all market rules with pagination and filtering
        /// Available to: Admin, Market Management Board Head, Local Government Representative, 
        /// Buyer, Proxy Shopper, Market Staff, Seller
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMarketRules([FromQuery] GetMarketRulesRequestDto request)
        {
            try
            {
                var result = await _marketRuleService.GetMarketRulesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving market rules", error = ex.Message });
            }
        }

        /// <summary>
        /// Get market rule by ID
        /// </summary>
        [HttpGet("{ruleId}")]
        public async Task<IActionResult> GetMarketRuleById(string ruleId)
        {
            try
            {
                var marketRule = await _marketRuleService.GetMarketRuleByIdAsync(ruleId);
                
                if (marketRule == null)
                    return NotFound(new { message = "Market rule not found" });

                return Ok(marketRule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving market rule", error = ex.Message });
            }
        }

        /// <summary>
        /// Get market rules by market ID
        /// </summary>
        [HttpGet("market/{marketId}")]
        public async Task<IActionResult> GetMarketRulesByMarketId(string marketId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _marketRuleService.GetMarketRulesByMarketIdAsync(marketId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving market rules", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new market rule
        /// Available to: Admin, Market Management Board Head, Local Government Representative, 
        /// Buyer, Proxy Shopper, Market Staff, Seller
        /// </summary>
        [HttpPost]
       // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMarketRule([FromBody] CreateMarketRuleDto createMarketRuleDto)
        {
            try
            {
                var marketRule = await _marketRuleService.CreateMarketRuleAsync(createMarketRuleDto);

                if (marketRule == null)
                    return BadRequest(new { message = "Failed to create market rule. Please check market ID." });

                return CreatedAtAction(nameof(GetMarketRuleById), new { ruleId = marketRule.Id }, marketRule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating market rule", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing market rule
        /// Available to: Admin, Market Management Board Head, Local Government Representative, 
        /// Buyer, Proxy Shopper, Market Staff, Seller
        /// </summary>
        [HttpPut("{ruleId}")]
        [Authorize(Roles = "Admin, MS, LGR, MMBH")]
        public async Task<IActionResult> UpdateMarketRule(string ruleId, [FromBody] UpdateMarketRuleDto updateMarketRuleDto)
        {
            try
            {
                var marketRule = await _marketRuleService.UpdateMarketRuleAsync(ruleId, updateMarketRuleDto);

                if (marketRule == null)
                    return NotFound(new { message = "Market rule not found or update failed" });

                return Ok(marketRule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating market rule", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a market rule
        /// Available to: Admin, Market Management Board Head, Local Government Representative, 
        /// Buyer, Proxy Shopper, Market Staff, Seller
        /// </summary>
        [HttpDelete("{ruleId}")]
        [Authorize(Roles = "Admin, MS, LGR, MMBH")]
        public async Task<IActionResult> DeleteMarketRule(string ruleId)
        {
            try
            {
                var success = await _marketRuleService.DeleteMarketRuleAsync(ruleId);

                if (!success)
                    return NotFound(new { message = "Market rule not found or delete failed" });

                return Ok(new { message = "Market rule deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting market rule", error = ex.Message });
            }
        }
    }
}
