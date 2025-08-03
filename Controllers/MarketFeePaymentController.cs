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

        [HttpPatch("{paymentId}/status")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> UpdateStatus(string paymentId, [FromQuery] string status)
        {
            var result = await _service.UpdatePaymentStatusAsync(paymentId, status);
            if (!result) return BadRequest("Cập nhật trạng thái thất bại");
            return Ok("Cập nhật trạng thái thành công");
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

        [HttpPost("admin/update-payment-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePaymentStatusByAdmin([FromBody] UpdatePaymentStatusDto dto)
        {
            try
            {
                var result = await _service.UpdatePaymentStatusByAdminAsync(dto);
                if (!result) return BadRequest("Cập nhật trạng thái thanh toán thất bại");
                return Ok("Cập nhật trạng thái thanh toán thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi cập nhật trạng thái: {ex.Message}");
            }
        }

        [HttpPatch("{paymentId}/admin-update-status")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> UpdatePaymentStatusByPaymentId(string paymentId, [FromBody] UpdatePaymentStatusByIdDto dto)
        {
            try
            {
                var payment = await _service.GetPaymentByIdAsync(paymentId);
                if (payment == null) return NotFound("Không tìm thấy thanh toán");

                var result = await _service.UpdatePaymentStatusAsync(paymentId, dto.PaymentStatus);
                if (!result) return BadRequest("Cập nhật trạng thái thất bại");
                return Ok("Cập nhật trạng thái thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi cập nhật trạng thái: {ex.Message}");
            }
        }

        // Admin endpoint: Get all stores with payment information
        [HttpGet("admin/stores-payment-overview")]
        // [Authorize(Roles = "Admin,MarketStaff")]
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
    }
}
