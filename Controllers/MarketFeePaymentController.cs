using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.Store;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketFeePaymentController : ControllerBase
    {
        private readonly IMarketFeePaymentService _service;
        public MarketFeePaymentController(IMarketFeePaymentService service)
        {
            _service = service;
        }

        [HttpGet("seller/{sellerId}")]
        [Authorize]
        public async Task<IActionResult> GetBySeller(string sellerId)
        {
            var result = await _service.GetPaymentsBySellerAsync(sellerId);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> GetAllPayments([FromQuery] GetAllMarketFeePaymentsRequestDto request)
        {
            try
            {
                var result = await _service.GetAllPaymentsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy danh sách thanh toán: {ex.Message}");
            }
        }

        [HttpGet("{paymentId}")]
        [Authorize]
        public async Task<IActionResult> GetById(string paymentId)
        {
            var result = await _service.GetPaymentByIdAsync(paymentId);
            if (result is null) return NotFound();
            return Ok(result!);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] MarketFeePaymentCreateDto dto)
        {
            var created = await _service.CreatePaymentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { paymentId = created.PaymentId }, created);
        }

        [HttpPost("market/sellers-payment-status")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> GetSellersPaymentStatus([FromBody] GetSellersPaymentStatusRequestDto request)
        {
            try
            {
                var result = await _service.GetSellersPaymentStatusAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy trạng thái thanh toán: {ex.Message}");
            }
        }

        // Admin endpoint: Get all stores with payment information
        [HttpGet("admin/stores-payment-overview")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> GetAllStoresWithPaymentInfo([FromQuery] GetAllStoresWithPaymentRequestDto request)
        {
            try
            {
                var result = await _service.GetAllStoresWithPaymentInfoAsync(request);
                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin thanh toán của các cửa hàng thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Lỗi khi lấy thông tin thanh toán: {ex.Message}",
                    data = (object?)null
                });
            }
        }

        // Admin endpoint: Update store payment status
        [HttpPatch("admin/payment/{paymentId}/update-status")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> UpdateStorePaymentStatus(string paymentId, [FromBody] UpdateStorePaymentStatusDto dto)
        {
            try
            {
                var result = await _service.UpdateStorePaymentStatusAsync(paymentId, dto);
                
                if (!result)
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy thanh toán hoặc cập nhật thất bại",
                        data = (object?)null
                    });

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật trạng thái thanh toán thành công",
                    data = (object?)null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Lỗi khi cập nhật trạng thái thanh toán: {ex.Message}",
                    data = (object?)null
                });
            }
        }

        [HttpPost("admin/create-payment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePaymentByAdmin([FromBody] AdminCreatePaymentDto dto)
        {
            try
            {
                var result = await _service.CreatePaymentByAdminAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = "Tạo thanh toán thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        [HttpPost("admin/create-payment-for-market")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePaymentForMarket([FromBody] AdminCreatePaymentForMarketDto dto)
        {
            try
            {
                var result = await _service.CreatePaymentForMarketAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = $"Tạo thanh toán cho chợ thành công. Đã tạo {result.SuccessfulPaymentsCreated}/{result.TotalSellersAffected} thanh toán",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }
    }
}
