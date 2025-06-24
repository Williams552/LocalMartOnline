using LocalMartOnline.Models.DTOs.Market;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Controllers
{
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
        public async Task<IActionResult> Create([FromBody] MarketCreateDto dto)
        {
            var market = await _marketService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = market.Id }, new
            {
                success = true,
                message = "Tạo chợ mới thành công",
                data = market
            });
        }

        // UC119: View Market List (Admin/Management)
        [HttpGet("admin")]
        [Authorize(Roles = "Admin,MarketManagementBoardHead,LocalGovernmentRepresentative")]
        public async Task<IActionResult> GetAllAdmin()
        {
            var markets = await _marketService.GetAllAsync();
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách tất cả chợ thành công",
                data = markets
            });
        }

        // View Active Markets (For regular users)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveMarkets()
        {
            var markets = await _marketService.GetActiveMarketsAsync();
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách chợ đang hoạt động thành công",
                data = markets
            });
        }

        // UC120: Update Market Info
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,MarketManagementBoardHead")]
        public async Task<IActionResult> Update(string id, [FromBody] MarketUpdateDto dto)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID không hợp lệ",
                    data = (object?)null
                });
            }

            var result = await _marketService.UpdateAsync(id, dto);
            if (!result) 
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy chợ để cập nhật",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Cập nhật thông tin chợ thành công",
                data = (object?)null
            });
        }

        // Toggle Market Status (Active/Suspended)
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Admin,MarketManagementBoardHead")]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID không hợp lệ",
                    data = (object?)null
                });
            }

            var result = await _marketService.ToggleStatusAsync(id);
            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy chợ để thay đổi trạng thái",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Thay đổi trạng thái chợ thành công",
                data = (object?)null
            });
        }

        // UC121: Delete Market
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,MarketManagementBoardHead")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID không hợp lệ",
                    data = (object?)null
                });
            }

            var result = await _marketService.DeleteAsync(id);
            if (!result) 
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể xóa chợ đang có gian hàng hoạt động",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Xóa chợ thành công",
                data = (object?)null
            });
        }

        // UC122: Search Markets (Admin)
        [HttpGet("admin/search")]
        [Authorize(Roles = "Admin,MarketManagementBoardHead,LocalGovernmentRepresentative")]
        public async Task<IActionResult> SearchAdmin([FromQuery] string keyword)
        {
            var markets = await _marketService.SearchAsync(keyword);
            return Ok(new
            {
                success = true,
                message = "Tìm kiếm tất cả chợ thành công",
                data = markets
            });
        }

        // UC122: Search Markets (Public - Only Active Markets)
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var markets = await _marketService.SearchActiveMarketsAsync(keyword);
            return Ok(new
            {
                success = true,
                message = "Tìm kiếm chợ đang hoạt động thành công",
                data = markets
            });
        }

        // UC123: Filter Markets (Admin)
        [HttpGet("admin/filter")]
        [Authorize(Roles = "Admin,MarketManagementBoardHead,LocalGovernmentRepresentative")]
        public async Task<IActionResult> FilterAdmin(
            [FromQuery] string? status,
            [FromQuery] string? area,
            [FromQuery] int? minStalls,
            [FromQuery] int? maxStalls)
        {
            var markets = await _marketService.FilterAsync(status, area, minStalls, maxStalls);
            return Ok(new
            {
                success = true,
                message = "Lọc danh sách tất cả chợ thành công",
                data = markets
            });
        }

        // UC123: Filter Markets (Public - Only Active Markets)
        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<IActionResult> Filter(
            [FromQuery] string? area,
            [FromQuery] int? minStalls,
            [FromQuery] int? maxStalls)
        {
            var markets = await _marketService.FilterActiveMarketsAsync(area, minStalls, maxStalls);
            return Ok(new
            {
                success = true,
                message = "Lọc danh sách chợ đang hoạt động thành công",
                data = markets
            });
        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID không hợp lệ",
                    data = (object?)null
                });
            }

            var market = await _marketService.GetByIdAsync(id);
            if (market == null) 
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy thông tin chợ",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Lấy thông tin chợ thành công",
                data = market
            });
        }
    }
}