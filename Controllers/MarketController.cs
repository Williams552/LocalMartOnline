using LocalMartOnline.Models.DTOs.Market;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class MarketController : ControllerBase
{
    private readonly IMarketService _marketService;

    public MarketController(IMarketService marketService)
    {
        _marketService = marketService;
    }

    // UC118: Create New Market
    [HttpPost]
    [Authorize(Roles = "Admin,MarketManagementBoardHead")]
    public async Task<ActionResult<MarketDto>> Create([FromBody] MarketCreateDto dto)
    {
        var market = await _marketService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = market.Id }, market);
    }

    // UC119: View Market List
    [HttpGet]
    //[Authorize(Roles = "Admin,MarketManagementBoardHead,LocalGovernmentRepresentative,Buyer,ProxyShopper,MarketStaff,Seller")]
    public async Task<ActionResult<IEnumerable<MarketDto>>> GetAll()
    {
        var markets = await _marketService.GetAllAsync();
        return Ok(markets);
    }

    // UC120: Update Market Info
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,MarketManagementBoardHead")]
    public async Task<IActionResult> Update(string id, [FromBody] MarketUpdateDto dto)
    {
        var result = await _marketService.UpdateAsync(id, dto);
        if (!result) return NotFound();
        return NoContent();
    }

    // UC121: Delete Market
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,MarketManagementBoardHead")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _marketService.DeleteAsync(id);
        if (!result) return BadRequest("Cannot delete market with active stalls.");
        return NoContent();
    }

    // UC122: Search Markets
    [HttpGet("search")]
    [Authorize(Roles = "Admin,MarketManagementBoardHead,LocalGovernmentRepresentative,Buyer,ProxyShopper,MarketStaff,Seller")]
    public async Task<ActionResult<IEnumerable<MarketDto>>> Search([FromQuery] string keyword)
    {
        var markets = await _marketService.SearchAsync(keyword);
        return Ok(markets);
    }

    // UC123: Filter Market List
    [HttpGet("filter")]
    [Authorize(Roles = "Admin,MarketManagementBoardHead,LocalGovernmentRepresentative,Buyer,ProxyShopper,MarketStaff,Seller")]
    public async Task<ActionResult<IEnumerable<MarketDto>>> Filter(
        [FromQuery] string? status,
        [FromQuery] string? area,
        [FromQuery] int? minStalls,
        [FromQuery] int? maxStalls)
    {
        var markets = await _marketService.FilterAsync(status, area, minStalls, maxStalls);
        return Ok(markets);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,MarketManagementBoardHead,LocalGovernmentRepresentative,Buyer,ProxyShopper,MarketStaff,Seller")]
    public async Task<ActionResult<MarketDto>> GetById(string id)
    {
        var market = await _marketService.GetByIdAsync(id);
        if (market == null) return NotFound();
        return Ok(market);
    }
}