using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Models.DTOs.ProductUnit;
using LocalMartOnline.Models;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductUnitController : ControllerBase
    {
        private readonly IProductUnitService _service;

        public ProductUnitController(IProductUnitService service)
        {
            _service = service;
        }

        // Get all active units for users and sellers
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveUnits()
        {
            var units = await _service.GetActiveUnitsAsync();
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách đơn vị đo lường thành công",
                data = units
            });
        }

        // Admin: Get all units with pagination
        [HttpGet("admin")]
        [Authorize(Roles = "Admin, MS, LGR, MMBH")]
        public async Task<IActionResult> GetAllPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetAllPagedAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách đơn vị đo lường thành công",
                data = result
            });
        }

        // Admin: Create new unit
        [HttpPost]
        [Authorize(Roles = "Admin, MS")]
        public async Task<IActionResult> Create([FromBody] ProductUnitCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    data = ModelState
                });
            }

            // Check if name already exists
            var nameExists = await _service.IsNameExistsAsync(dto.Name);
            if (nameExists)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tên đơn vị đã tồn tại",
                    data = (object?)null
                });
            }

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new
            {
                success = true,
                message = "Tạo đơn vị đo lường mới thành công",
                data = created
            });
        }

        // Admin: Update unit
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, MS")]
        public async Task<IActionResult> Update(string id, [FromBody] ProductUnitUpdateDto dto)
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

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    data = ModelState
                });
            }

            // Check if name already exists (excluding current unit)
            var nameExists = await _service.IsNameExistsAsync(dto.Name, id);
            if (nameExists)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tên đơn vị đã tồn tại",
                    data = (object?)null
                });
            }

            var result = await _service.UpdateAsync(id, dto);
            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy đơn vị để cập nhật",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Cập nhật đơn vị thành công",
                data = (object?)null
            });
        }

        // Admin: Toggle unit status
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Admin, MS")]
        public async Task<IActionResult> Toggle(string id)
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

            var result = await _service.ToggleAsync(id);
            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy đơn vị để thay đổi trạng thái",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Thay đổi trạng thái đơn vị thành công",
                data = (object?)null
            });
        }

        // Admin: Delete unit (soft delete)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, MS")]
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

            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy đơn vị để xóa",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Xóa đơn vị thành công",
                data = (object?)null
            });
        }

        // Get unit by ID
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

            var unit = await _service.GetByIdAsync(id);
            if (unit == null)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy đơn vị",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Lấy thông tin đơn vị thành công",
                data = unit
            });
        }

        // Search units for users
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Từ khóa tìm kiếm không được để trống",
                    data = (object?)null
                });
            }

            var units = await _service.SearchByNameAsync(name);
            return Ok(new
            {
                success = true,
                message = "Tìm kiếm đơn vị thành công",
                data = units
            });
        }

        // Admin: Search all units
        [HttpGet("admin/search")]
        [Authorize(Roles = "Admin, MS")]
        public async Task<IActionResult> SearchAdmin([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Từ khóa tìm kiếm không được để trống",
                    data = (object?)null
                });
            }

            var units = await _service.SearchByNameAdminAsync(name);
            return Ok(new
            {
                success = true,
                message = "Tìm kiếm tất cả đơn vị thành công",
                data = units
            });
        }

        // Get units by type
        [HttpGet("type/{unitType}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByType(UnitType unitType, [FromQuery] bool activeOnly = true)
        {
            var units = await _service.GetByUnitTypeAsync(unitType, activeOnly);
            return Ok(new
            {
                success = true,
                message = $"Lấy danh sách đơn vị loại {unitType} thành công",
                data = units
            });
        }

        // Admin: Reorder units
        [HttpPost("reorder")]
        [Authorize(Roles = "Admin, MS")]
        public async Task<IActionResult> Reorder([FromBody] List<ProductUnitReorderDto> reorderList)
        {
            if (reorderList == null || !reorderList.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Danh sách sắp xếp không hợp lệ",
                    data = (object?)null
                });
            }

            var result = await _service.ReorderUnitsAsync(reorderList);
            if (!result)
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể sắp xếp lại đơn vị",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Sắp xếp lại đơn vị thành công",
                data = (object?)null
            });
        }

        // Get all unit types
        [HttpGet("types")]
        [AllowAnonymous]
        public IActionResult GetUnitTypes()
        {
            var unitTypes = Enum.GetValues<UnitType>()
                .Select(ut => new
                {
                    Value = ut,
                    Name = ut.ToString(),
                    DisplayName = GetUnitTypeDisplayName(ut)
                })
                .ToList();

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách loại đơn vị thành công",
                data = unitTypes
            });
        }

        private static string GetUnitTypeDisplayName(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Weight => "Khối lượng",
                UnitType.Volume => "Thể tích",
                UnitType.Count => "Số lượng",
                UnitType.Length => "Chiều dài",
                _ => unitType.ToString()
            };
        }
    }
}