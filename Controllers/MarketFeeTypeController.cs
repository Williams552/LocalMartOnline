using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Models.DTOs.MarketFee;
using LocalMartOnline.Services.Interface;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketFeeTypeController : ControllerBase
    {
        private readonly IMarketFeeTypeService _marketFeeTypeService;

        public MarketFeeTypeController(IMarketFeeTypeService marketFeeTypeService)
        {
            _marketFeeTypeService = marketFeeTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMarketFeeTypes()
        {
            try
            {
                var result = await _marketFeeTypeService.GetAllMarketFeeTypesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving market fee types", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMarketFeeTypeById(string id)
        {
            try
            {
                var result = await _marketFeeTypeService.GetMarketFeeTypeByIdAsync(id);
                
                if (result == null)
                    return NotFound(new { message = "Market fee type not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving market fee type", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMarketFeeType([FromBody] CreateMarketFeeTypeDto createDto)
        {
            try
            {
                var result = await _marketFeeTypeService.CreateMarketFeeTypeAsync(createDto);
                return CreatedAtAction(nameof(GetMarketFeeTypeById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating market fee type", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMarketFeeType(string id, [FromBody] UpdateMarketFeeTypeDto updateDto)
        {
            try
            {
                var result = await _marketFeeTypeService.UpdateMarketFeeTypeAsync(id, updateDto);
                
                if (result == null)
                    return NotFound(new { message = "Market fee type not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating market fee type", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMarketFeeType(string id)
        {
            try
            {
                var success = await _marketFeeTypeService.DeleteMarketFeeTypeAsync(id);
                
                if (!success)
                    return NotFound(new { message = "Market fee type not found" });

                return Ok(new { message = "Market fee type deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting market fee type", error = ex.Message });
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreMarketFeeType(string id)
        {
            try
            {
                var success = await _marketFeeTypeService.RestoreMarketFeeTypeAsync(id);
                
                if (!success)
                    return NotFound(new { message = "Market fee type not found" });

                return Ok(new { message = "Market fee type restored successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error restoring market fee type", error = ex.Message });
            }
        }
    }
}
