using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Models.DTOs.Store;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        public StoreController(IStoreService storeService, IProductService productService)
        {
            _storeService = storeService;
            _productService = productService;
        }

        // UC030: Open Store
        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreateStore([FromBody] StoreCreateDto dto)
        {
            try
            {
                var result = await _storeService.CreateStoreAsync(dto);
                return CreatedAtAction(nameof(GetStoreProfile), new { id = result.Id }, new
                {
                    success = true,
                    message = "Tạo gian hàng thành công",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        //// UC031: Close Store
        //[HttpPatch("{id}/close")]
        //[Authorize(Roles = "Seller")]
        //public async Task<IActionResult> CloseStore(string id)
        //{
        //    if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
        //    {
        //        return BadRequest(new
        //        {
        //            success = false,
        //            message = "ID gian hàng không hợp lệ",
        //            data = (object?)null
        //        });
        //    }

        //    var result = await _storeService.CloseStoreAsync(id);
        //    if (!result)
        //        return NotFound(new
        //        {
        //            success = false,
        //            message = "Không tìm thấy gian hàng",
        //            data = (object?)null
        //        });

        //    return Ok(new
        //    {
        //        success = true,
        //        message = "Đóng cửa gian hàng thành công",
        //        data = (object?)null
        //    });
        //}

        // Toggle Store Status (Open/Closed)
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> ToggleStoreStatus(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID gian hàng không hợp lệ",
                    data = (object?)null
                });
            }

            var result = await _storeService.ToggleStoreStatusAsync(id);
            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Không thể thay đổi trạng thái gian hàng",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Thay đổi trạng thái gian hàng thành công",
                data = (object?)null
            });
        }

        // UC032: Update Store
        [HttpPut("{id}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UpdateStore(string id, [FromBody] StoreUpdateDto dto)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID gian hàng không hợp lệ",
                    data = (object?)null
                });
            }

            var result = await _storeService.UpdateStoreAsync(id, dto);
            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy gian hàng",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Cập nhật thông tin gian hàng thành công",
                data = (object?)null
            });
        }

        // UC037: Follow Store
        [HttpPost("{storeId}/follow")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> FollowStore(long storeId, [FromQuery] long userId)
        {
            var result = await _storeService.FollowStoreAsync(userId, storeId);
            if (!result)
                return BadRequest(new
                {
                    success = false,
                    message = "Đã theo dõi hoặc không hợp lệ",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Theo dõi gian hàng thành công",
                data = (object?)null
            });
        }

        // UC039: Unfollow Store
        [HttpPost("{storeId}/unfollow")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> UnfollowStore(long storeId, [FromQuery] long userId)
        {
            var result = await _storeService.UnfollowStoreAsync(userId, storeId);
            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy theo dõi",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Hủy theo dõi gian hàng thành công",
                data = (object?)null
            });
        }

        // UC038: View Following Store List
        [HttpGet("following")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> GetFollowingStores([FromQuery] long userId)
        {
            var stores = await _storeService.GetFollowingStoresAsync(userId);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách gian hàng đang theo dõi thành công",
                data = stores
            });
        }

        // UC040: View Store Profile
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStoreProfile(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID gian hàng không hợp lệ",
                    data = (object?)null
                });
            }

            var store = await _storeService.GetStoreProfileAsync(id);
            if (store == null)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy gian hàng",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Lấy thông tin gian hàng thành công",
                data = store
            });
        }

        // View All Stores of a Market
        [HttpGet("market/{marketId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStoresByMarket(string marketId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(marketId, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID chợ không hợp lệ",
                    data = (object?)null
                });
            }

            var stores = await _storeService.GetActiveStoresByMarketIdAsync(marketId, page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách gian hàng trong chợ thành công",
                data = stores
            });
        }

        // UC025: View All Stores (Admin)
        [HttpGet("admin")]
        [Authorize(Roles = "Admin,MarketManagementBoardHead")]
        public async Task<IActionResult> GetAllStores([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _storeService.GetAllStoresAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách tất cả gian hàng thành công",
                data = result
            });
        }

        // UC026: View Suspended Stores
        [HttpGet("suspended")]
        [Authorize(Roles = "Admin,MarketManagementBoardHead,LocalGovernmentRepresentative")]
        public async Task<IActionResult> GetSuspendedStores([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _storeService.GetSuspendedStoresAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách gian hàng bị đình chỉ thành công",
                data = result
            });
        }

        // UC027: Suspend Store
        [HttpPatch("{id}/suspend")]
        [Authorize(Roles = "Admin,MarketManagementBoardHead")]
        public async Task<IActionResult> SuspendStore(string id, [FromBody] SuspendStoreDto dto)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID gian hàng không hợp lệ",
                    data = (object?)null
                });
            }

            var result = await _storeService.SuspendStoreAsync(id, dto.Reason);
            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy gian hàng",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Đình chỉ gian hàng thành công",
                data = (object?)null
            });
        }

        // UC028: Reactivate Store
        [HttpPatch("{id}/reactivate")]
        [Authorize(Roles = "Admin,MarketManagementBoardHead")]
        public async Task<IActionResult> ReactivateStore(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID gian hàng không hợp lệ",
                    data = (object?)null
                });
            }

            var result = await _storeService.ReactivateStoreAsync(id);
            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy gian hàng",
                    data = (object?)null
                });

            return Ok(new
            {
                success = true,
                message = "Kích hoạt gian hàng thành công",
                data = (object?)null
            });
        }

        // Xem tất cả sản phẩm trong store (UC044)
        [HttpGet("{id}/products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStoreProducts(
          string id,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 20)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID gian hàng không hợp lệ",
                    data = (object?)null
                });
            }

            var result = await _productService.GetProductsByStoreAsync(id, page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách sản phẩm trong gian hàng thành công",
                data = result
            });
        }

        // Tìm kiếm và lọc cửa hàng (cho người dùng)
        [HttpPost("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchStores([FromBody] StoreSearchFilterDto filter)
        {
            var result = await _storeService.SearchStoresAsync(filter, false);
            return Ok(new
            {
                success = true,
                message = "Tìm kiếm cửa hàng thành công",
                data = result
            });
        }

        // Tìm kiếm và lọc cửa hàng (cho admin)
        [HttpPost("admin/search")]
        [Authorize(Roles = "Admin,MarketManagementBoardHead")]
        public async Task<IActionResult> SearchStoresAdmin([FromBody] StoreSearchFilterDto filter)
        {
            var result = await _storeService.SearchStoresAsync(filter, true);
            return Ok(new
            {
                success = true,
                message = "Tìm kiếm tất cả cửa hàng thành công",
                data = result
            });
        }

        // Tìm cửa hàng gần đây
        [HttpGet("nearby")]
        [AllowAnonymous]
        public async Task<IActionResult> FindNearbyStores(
            [FromQuery] decimal latitude,
            [FromQuery] decimal longitude,
            [FromQuery] decimal maxDistance = 5, // Mặc định 5km
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _storeService.FindStoresNearbyAsync(
                latitude, longitude, maxDistance, page, pageSize);
                
            return Ok(new
            {
                success = true,
                message = "Tìm kiếm cửa hàng gần đây thành công",
                data = result
            });
        }

        // Xem tất cả gian hàng đang mở (cho user)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllActiveStores([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _storeService.GetActiveStoresAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách gian hàng đang hoạt động thành công",
                data = result
            });
        }
    }
}