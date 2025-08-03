using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.ProxyShopping;
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
        private readonly IRepository<Store> _storeRepo;
        public ProxyShopperController(IProxyShopperService proxyShopperService, IRepository<User> userRepo, IRepository<Store> storeRepo)
        {
            _proxyShopperService = proxyShopperService;
            _userRepo = userRepo;
            _storeRepo = storeRepo;
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
        [Authorize(Roles = "Proxy Shopper,Buyer")]
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
        [Authorize(Roles = "Buyer")]
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
            
            // Lấy danh sách buyerId và storeId
            var buyerIds = requests.Select(r => r.BuyerId).Distinct().ToList();
            var storeIds = requests.Select(r => r.StoreId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            
            // Lấy thông tin users và stores
            var users = await _userRepo.FindManyAsync(u => u.Id != null && buyerIds.Contains(u.Id!));
            var stores = await _storeRepo.FindManyAsync(s => s.Id != null && storeIds.Contains(s.Id!));
            
            var userDict = users.Where(u => u.Id != null).ToDictionary(u => u.Id!, u => u);
            var storeDict = stores.Where(s => s.Id != null).ToDictionary(s => s.Id!, s => s);
            
            var result = requests.Select(r => new ProxyRequestResponseDto
            {
                Id = r.Id,
                Items = r.Items,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                BuyerName = userDict.TryGetValue(r.BuyerId, out var user) ? user.FullName : null,
                BuyerEmail = userDict.TryGetValue(r.BuyerId, out var user2) ? user2.Email : null,
                BuyerPhone = userDict.TryGetValue(r.BuyerId, out var user3) ? user3.PhoneNumber : null,
                StoreId = r.StoreId,
                StoreName = !string.IsNullOrEmpty(r.StoreId) && storeDict.TryGetValue(r.StoreId, out var store) ? store.Name : null
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
                Console.WriteLine($"[DEBUG] SendProposal - OrderId: {orderId}");
                Console.WriteLine($"[DEBUG] SendProposal - ProxyShopperId: {proxyShopperId}");
                Console.WriteLine($"[DEBUG] SendProposal - Items Count: {dto.Items.Count}");
                Console.WriteLine($"[DEBUG] SendProposal - TotalProductPrice: {dto.TotalProductPrice}");
                Console.WriteLine($"[DEBUG] SendProposal - ProxyFee: {dto.ProxyFee}");
                Console.WriteLine($"[DEBUG] SendProposal - TotalAmount: {dto.TotalAmount}");

                var ok = await _proxyShopperService.SendProposalAsync(orderId, dto);

                if (ok)
                {
                    Console.WriteLine($"[DEBUG] SendProposal - Success for OrderId: {orderId}");
                    return Ok(new { message = "Đề xuất đã được gửi thành công.", orderId });
                }
                else
                {
                    Console.WriteLine($"[DEBUG] SendProposal - Failed for OrderId: {orderId}");
                    return BadRequest("Không thể gửi đề xuất. Đơn hàng có thể không tồn tại hoặc không ở trạng thái Draft.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendProposal - Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] SendProposal - StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 5. Buyer duyệt & thanh toán
        [HttpPost("orders/{orderId}/approve-pay")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> BuyerApproveAndPay(string orderId)
        {
            var buyerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("Không tìm thấy userId trong token.");
            var ok = await _proxyShopperService.BuyerApproveAndPayAsync(orderId, buyerId);
            return ok ? Ok() : BadRequest("Chỉ được duyệt khi đơn ở trạng thái chờ thanh toán.");
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

                // Validate image URLs
                if (dto.ImageUrls != null && dto.ImageUrls.Any())
                {
                    var invalidUrls = dto.ImageUrls.Where(url => string.IsNullOrWhiteSpace(url)).ToList();
                    if (invalidUrls.Any())
                    {
                        return BadRequest("Có URL hình ảnh không hợp lệ (trống hoặc null).");
                    }

                    // Optional: Basic URL format validation
                    var malformedUrls = dto.ImageUrls.Where(url =>
                        !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) ||
                        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                    ).ToList();

                    if (malformedUrls.Any())
                    {
                        return BadRequest($"Có {malformedUrls.Count} URL hình ảnh không đúng định dạng.");
                    }
                }

                // Get current proxy shopper ID for authorization check
                var proxyShopperId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(proxyShopperId))
                    return Unauthorized("Không tìm thấy userId trong token.");

                Console.WriteLine($"[DEBUG] UploadBoughtItems - Proxy {proxyShopperId} uploading {dto.ImageUrls?.Count ?? 0} images for order {orderId}");

                var ok = await _proxyShopperService.UploadBoughtItemsAsync(orderId, dto.ImageUrls, dto.Note);

                if (ok)
                {
                    return Ok(new
                    {
                        message = "Upload ảnh chứng từ thành công.",
                        orderId = orderId,
                        imageCount = dto.ImageUrls?.Count ?? 0,
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
                Console.WriteLine($"[ERROR] UploadBoughtItems - Exception: {ex.Message}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 8. Buyer xác nhận hoàn tất đơn
        [HttpPost("orders/{orderId}/confirm-delivery")]
        [Authorize(Roles = "Buyer")]
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
