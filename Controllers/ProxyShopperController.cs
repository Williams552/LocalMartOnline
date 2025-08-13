using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.ProxyShopping;
using LocalMartOnline.Models.DTOs.Seller;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Repositories;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProxyShopperController : ControllerBase
    {
        private readonly IProxyShopperService _proxyShopperService;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Market> _marketRepo;
        public ProxyShopperController(IProxyShopperService proxyShopperService, IRepository<User> userRepo, IRepository<Market> marketRepo)
        {
            _proxyShopperService = proxyShopperService;
            _userRepo = userRepo;
            _marketRepo = marketRepo;
        }

        // ADMIN: Lấy danh sách tất cả proxy requests
        [HttpGet("proxy-requests")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllProxyRequests()
        {
            var requests = await _proxyShopperService.GetAllProxyRequestsAsync();
            return Ok(requests);
        }

        // ADMIN: Lấy chi tiết proxy request theo id
        [HttpGet("proxy-requests/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProxyRequestById(string id)
        {
            var request = await _proxyShopperService.GetProxyRequestDetailForAdminAsync(id);
            if (request == null) return NotFound();
            return Ok(request);
        }

        // ADMIN: Cập nhật trạng thái proxy request
        [HttpPatch("proxy-requests/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProxyRequestStatus(string id, [FromBody] UpdateProxyRequestStatusDto dto)
        {
            var ok = await _proxyShopperService.UpdateProxyRequestStatusAsync(id, dto.Status);
            return ok ? Ok() : BadRequest();
        }

        // ADMIN: Cập nhật trạng thái proxy order
        [HttpPatch("proxy-orders/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProxyOrderStatus(string id, [FromBody] UpdateProxyOrderStatusDto dto)
        {
            var ok = await _proxyShopperService.UpdateProxyOrderStatusAsync(id, dto.Status);
            return ok ? Ok() : BadRequest();
        }


        [HttpPost("register")]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] ProxyShopperRegistrationRequestDTO dto)
        {
            var userId = ""; // Lấy userId từ Claims
            await _proxyShopperService.RegisterProxyShopperAsync(dto, userId);
            return Ok(new { success = true });
        }

        [HttpPost("orders/{orderId}/accept")]
        [Authorize]
        public IActionResult AcceptOrder(string orderId)
        {
            // Method AcceptOrderAsync không tồn tại, có thể cần implement hoặc sử dụng method khác
            return Ok(new { success = true, message = "Method chưa được implement" });
        }

        // Proxy lấy các request đã nhận
        [HttpGet("requests/my-accepted")]
        [Authorize(Roles = "Proxy Shopper")]
        public async Task<IActionResult> GetMyAcceptedRequests()
        {
            var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(proxyShopperId))
                return Unauthorized("Không tìm thấy userId trong token.");

            var result = await _proxyShopperService.GetMyAcceptedRequestsAsync(proxyShopperId);
            return Ok(result);
        }

        // Lấy danh sách requests cho cả Buyer và Proxy Shopper
        [HttpGet("requests/my-requests")]
        [Authorize(Roles = "Proxy Shopper,Buyer, Seller")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Không tìm thấy userId trong token.");

            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userRole))
                return Unauthorized("Không tìm thấy role trong token.");

            var result = await _proxyShopperService.GetMyRequestsAsync(userId, userRole);
            return Ok(result);
        }

        // 1. Buyer tạo request
        [HttpPost("requests")]
        [Authorize(Roles = "Buyer, Seller")]
        public async Task<IActionResult> CreateProxyRequest([FromBody] ProxyRequestDto proxyRequest)
        {
            var buyerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("Không tìm thấy userId trong token.");
            var requestId = await _proxyShopperService.CreateProxyRequestAsync(buyerId, proxyRequest);
            return Ok(new { requestId });
        }

        // 2. Proxy lấy danh sách request còn Open trong chợ đã đăng ký
        [HttpGet("requests/available")]
        [Authorize(Roles = "Proxy Shopper")]
        public async Task<IActionResult> GetAvailableRequests()
        {
            var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(proxyShopperId))
                return Unauthorized("Không tìm thấy userId trong token.");

            var requests = await _proxyShopperService.GetAvailableRequestsForProxyAsync(proxyShopperId);

            // Lấy danh sách buyerId và marketId
            var buyerIds = requests.Select(r => r.BuyerId).Distinct().ToList();
            var marketIds = requests.Select(r => r.MarketId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

            // Lấy thông tin users và markets
            var users = await _userRepo.FindManyAsync(u => u.Id != null && buyerIds.Contains(u.Id!));
            var markets = await _marketRepo.FindManyAsync(m => m.Id != null && marketIds.Contains(m.Id!));

            var userDict = users.Where(u => u.Id != null).ToDictionary(u => u.Id!, u => u);
            var marketDict = markets.Where(m => m.Id != null).ToDictionary(m => m.Id!, m => m);

            var result = requests.Select(r => new ProxyRequestResponseDto
            {
                Id = r.Id,
                Items = r.Items,
                DeliveryAddress = r.DeliveryAddress,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                BuyerName = userDict.TryGetValue(r.BuyerId, out var user) ? user.FullName : null,
                BuyerEmail = userDict.TryGetValue(r.BuyerId, out var user2) ? user2.Email : null,
                BuyerPhone = userDict.TryGetValue(r.BuyerId, out var user3) ? user3.PhoneNumber : null,
                MarketId = r.MarketId,
                MarketName = !string.IsNullOrEmpty(r.MarketId) && marketDict.TryGetValue(r.MarketId, out var market) ? market.Name : null
            }).ToList();
            return Ok(result);
        }

        // 3. Proxy nhận request & tạo order
        [HttpPost("requests/{requestId}/accept")]
        [Authorize(Roles = "Proxy Shopper")]
        public async Task<IActionResult> AcceptRequest(string requestId)
        {
            var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(proxyShopperId))
                return Unauthorized("Không tìm thấy userId trong token.");
            var orderId = await _proxyShopperService.AcceptRequestAndCreateOrderAsync(requestId, proxyShopperId);
            if (orderId == null) return BadRequest("Yêu cầu đã được nhận bởi người khác hoặc không còn hiệu lực.");
            return Ok(new { orderId });
        }

        // 4. Proxy gửi đề xuất đơn hàng (products + tổng giá + phí)
        [HttpPost("orders/{orderId}/proposal")]
        [Authorize(Roles = "Proxy Shopper")]
        public async Task<IActionResult> SendProposal(string orderId, [FromBody] ProxyShoppingProposalDTO dto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(orderId))
                    return BadRequest("OrderId không được để trống.");

                if (dto == null)
                    return BadRequest("Dữ liệu đề xuất không được để trống.");

                if (dto.Items == null || !dto.Items.Any())
                    return BadRequest("Danh sách sản phẩm không được để trống.");

                if (dto.TotalProductPrice <= 0)
                    return BadRequest("Tổng tiền sản phẩm phải lớn hơn 0.");

                if (dto.ProxyFee < 0)
                    return BadRequest("Phí dịch vụ không được âm.");

                // Validate items
                for (int i = 0; i < dto.Items.Count; i++)
                {
                    var item = dto.Items[i];
                    if (string.IsNullOrEmpty(item.Id))
                        return BadRequest($"Sản phẩm thứ {i + 1}: ID không được để trống.");

                    if (string.IsNullOrEmpty(item.Name))
                        return BadRequest($"Sản phẩm thứ {i + 1}: Tên không được để trống.");

                    if (item.Quantity <= 0)
                        return BadRequest($"Sản phẩm thứ {i + 1}: Số lượng phải lớn hơn 0.");

                    if (item.Price <= 0)
                        return BadRequest($"Sản phẩm thứ {i + 1}: Giá phải lớn hơn 0.");

                    if (string.IsNullOrEmpty(item.Unit))
                        return BadRequest($"Sản phẩm thứ {i + 1}: Đơn vị không được để trống.");
                }

                // Get current proxy shopper ID for authorization check
                var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(proxyShopperId))
                    return Unauthorized("Không tìm thấy userId trong token.");

                // Log proposal details
                var ok = await _proxyShopperService.SendProposalAsync(orderId, dto);

                if (ok)
                {
                    return Ok(new { message = "Đề xuất đã được gửi thành công.", orderId });
                }
                else
                {
                    return BadRequest("Không thể gửi đề xuất. Đơn hàng có thể không tồn tại hoặc không ở trạng thái Draft.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 5. Buyer duyệt & thanh toán
        [HttpPost("orders/{orderId}/approve-pay")]
        [Authorize(Roles = "Buyer, Seller")]
        public async Task<IActionResult> BuyerApproveAndPay(string orderId)
        {
            var buyerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("Không tìm thấy userId trong token.");
            var ok = await _proxyShopperService.BuyerApproveAndPayAsync(orderId, buyerId);
            return ok ? Ok() : BadRequest("Chỉ được duyệt khi đơn ở trạng thái chờ thanh toán.");
        }

        // 5.1. Buyer từ chối đề xuất
        [HttpPost("orders/{orderId}/reject-proposal")]
        [Authorize(Roles = "Buyer, Seller")]
        public async Task<IActionResult> RejectProposal(string orderId, [FromBody] RejectProposalDTO dto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(orderId))
                    return BadRequest("OrderId không được để trống.");

                if (dto == null || string.IsNullOrEmpty(dto.Reason))
                    return BadRequest("Lý do từ chối không được để trống.");

                var buyerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(buyerId))
                    return Unauthorized("Không tìm thấy userId trong token.");

                var result = await _proxyShopperService.RejectProposalAsync(orderId, buyerId, dto.Reason);

                if (result)
                {
                    return Ok(new { message = "Từ chối đề xuất thành công. Proxy shopper có thể tạo đề xuất mới.", orderId });
                }
                else
                {
                    return BadRequest("Không thể từ chối đề xuất. Đơn hàng có thể không tồn tại, không thuộc về bạn, hoặc không ở trạng thái chờ duyệt.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 6. Proxy bắt đầu mua hàng
        [HttpPost("orders/{orderId}/start-shopping")]
        [Authorize(Roles = "Proxy Shopper")]
        public async Task<IActionResult> StartShopping(string orderId)
        {
            var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(proxyShopperId))
                return Unauthorized("Không tìm thấy userId trong token.");
            var ok = await _proxyShopperService.StartShoppingAsync(orderId, proxyShopperId);
            return ok ? Ok() : BadRequest("Không thể bắt đầu mua hàng ở trạng thái này.");
        }

        // 7. Proxy upload ảnh hàng hóa (hóa đơn, sản phẩm thực tế...)
        [HttpPost("orders/{orderId}/proof")]
        [Authorize(Roles = "Proxy Shopper")]
        public async Task<IActionResult> UploadBoughtItems(string orderId, [FromBody] UploadBoughtItemsDTO dto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(orderId))
                    return BadRequest("OrderId không được để trống.");

                if (dto == null)
                    return BadRequest("Dữ liệu upload không được để trống.");

                // Get current proxy shopper ID for authorization check
                var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(proxyShopperId))
                    return Unauthorized("Không tìm thấy userId trong token.");

                var ok = await _proxyShopperService.UploadBoughtItemsAsync(orderId, dto.ImageUrls, dto.Note);

                if (ok)
                {
                    return Ok(new
                    {
                        message = "Upload ảnh chứng từ thành công.",
                        orderId = orderId,
                        imageUrls = dto.ImageUrls,
                        uploadedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest("Không thể upload ảnh cho đơn này. Đơn hàng có thể không tồn tại hoặc không ở trạng thái đang mua hàng.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 8. Buyer xác nhận hoàn tất đơn
        [HttpPost("orders/{orderId}/confirm-delivery")]
        [Authorize(Roles = "Buyer, Seller")]
        public async Task<IActionResult> ConfirmDelivery(string orderId)
        {
            try
            {
                var buyerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(buyerId))
                    return Unauthorized("Không tìm thấy userId trong token.");
                var ok = await _proxyShopperService.ConfirmDeliveryAsync(orderId, buyerId);
                if (ok)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Không thể xác nhận giao hàng cho đơn này.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 9. Proxy hủy đơn, mở lại request
        [HttpPost("orders/{orderId}/cancel")]
        [Authorize(Roles = "Proxy Shopper")]
        public async Task<IActionResult> CancelOrder(string orderId, [FromBody] CancelOrderDTO dto)
        {
            var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(proxyShopperId))
                return Unauthorized("Không tìm thấy userId trong token.");
            var ok = await _proxyShopperService.CancelOrderAsync(orderId, proxyShopperId, dto.Reason);
            return ok ? Ok() : BadRequest("Không thể hủy đơn hàng này.");
        }

        // Lấy thông tin chi tiết của một request theo ID
        [HttpGet("requests/{requestId}")]
        [Authorize(Roles = "Proxy Shopper")]
        public async Task<IActionResult> GetRequestById(string requestId)
        {
            var request = await _proxyShopperService.GetRequestByIdAsync(requestId);
            if (request == null) return NotFound("Không tìm thấy yêu cầu này.");
            return Ok(request);
        }


        // Buyer hủy request trước khi có proxy shopper nhận
        [HttpPost("requests/{requestId}/cancel")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> CancelRequest(string requestId)
        {
            try
            {
                var buyerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(buyerId))
                    return Unauthorized("Không tìm thấy userId trong token.");

                var result = await _proxyShopperService.CancelRequestAsync(requestId, buyerId);
                
                if (result)
                {
                    return Ok(new { message = "Hủy yêu cầu thành công.", requestId });
                }
                else
                {
                    return BadRequest("Không thể hủy yêu cầu này. Yêu cầu có thể không tồn tại, không thuộc về bạn, hoặc đã có proxy shopper nhận.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("products/advanced-search")]
        [Authorize(Roles = "Proxy Shopper")]
        public async Task<IActionResult> AdvancedProductSearch(
            string query,
            double wPrice = 0.25,
            double wReputation = 0.25,
            double wSold = 0.25,
            double wStock = 0.25)
        {
            var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(proxyShopperId))
                return Unauthorized("Không tìm thấy userId trong token.");

            var result = await _proxyShopperService.AdvancedProductSearchAsync(proxyShopperId, query, wPrice, wReputation, wSold, wStock);
            return Ok(result);
        }
    }
}
