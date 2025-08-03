using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Payment;
using LocalMartOnline.Services;
using Microsoft.AspNetCore.Authorization;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VnPayController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        public VnPayController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        [HttpPost("create-payment")]
        public IActionResult CreatePayment([FromBody] PaymentInformationModel model)
        {
            var paymentUrl = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Ok(new { paymentUrl });
        }

        [HttpGet("callback")]
        public IActionResult PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            // Xử lý kết quả thanh toán ở đây (lưu DB, gửi thông báo, ...)
            return Ok(response);
        }

        // === MarketFeePayment endpoints ===

        // Seller xem danh sách payments pending
        [HttpGet("marketfee/pending/{sellerId}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> GetPendingMarketFeePayments(string sellerId)
        {
            try
            {
                var result = await _vnPayService.GetPendingPaymentsAsync(sellerId);
                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách thanh toán pending thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Lỗi khi lấy danh sách thanh toán: {ex.Message}",
                    data = (object?)null
                });
            }
        }

        // Tạo URL thanh toán cho MarketFeePayment
        [HttpPost("marketfee/create-payment-url/{paymentId}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreateMarketFeePaymentUrl(string paymentId)
        {
            try
            {
                var request = new CreatePaymentUrlRequestDto
                {
                    PaymentId = paymentId,
                    ReturnUrl = "" // Add return URL if needed
                };
                var result = await _vnPayService.CreateMarketFeePaymentUrlAsync(request, HttpContext);
                return Ok(new
                {
                    success = true,
                    message = "Tạo URL thanh toán thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Lỗi khi tạo URL thanh toán: {ex.Message}",
                    data = (object?)null
                });
            }
        }

        // Callback xử lý kết quả thanh toán MarketFeePayment
        [HttpGet("marketfee-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> MarketFeePaymentCallback()
        {
            try
            {
                var result = await _vnPayService.ProcessMarketFeePaymentCallbackAsync(Request.Query);
                
                if (result)
                {
                    // Redirect về trang thành công
                    return Redirect("/payment-success");
                }
                else
                {
                    // Redirect về trang thất bại
                    return Redirect("/payment-failed");
                }
            }
            catch (Exception ex)
            {
                return Redirect($"/payment-error?message={ex.Message}");
            }
        }
    }
}
