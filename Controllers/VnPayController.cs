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
        // [Authorize(Roles = "Seller")]
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
        // [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreateMarketFeePaymentUrl(string paymentId)
        {
            try
            {
                // Validate paymentId
                if (string.IsNullOrEmpty(paymentId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "PaymentId không được để trống",
                        data = (object?)null
                    });
                }
                
                var request = new CreatePaymentUrlRequestDto
                {
                    PaymentId = paymentId,
                    ReturnUrl = "" // Add return URL if needed
                };

                // Set timeout for the operation
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                var result = await _vnPayService.CreateMarketFeePaymentUrlAsync(request, HttpContext);
                
                return Ok(new
                {
                    success = true,
                    message = "Tạo URL thanh toán thành công",
                    data = result
                });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, new
                {
                    success = false,
                    message = "Yêu cầu quá thời gian. Vui lòng thử lại sau.",
                    data = (object?)null
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
                
                // Redirect đến trang payments của seller
                var redirectUrl = "http://localhost:3000/seller/payments";
                
                if (result)
                {
                    // Thanh toán thành công - encode message để tránh lỗi non-ASCII
                    var successMessage = Uri.EscapeDataString("Thanh toán thành công");
                    return Redirect($"{redirectUrl}");
                }
                else
                {
                    return Redirect($"{redirectUrl}");
                }
            }
            catch
            {
                // Lỗi xử lý - encode message để tránh lỗi non-ASCII
                var redirectUrl = "http://localhost:3000/seller/payments";
                return Redirect($"{redirectUrl}");
            }
        }

        // Kiểm tra trạng thái thanh toán
        [HttpGet("marketfee/payment-status/{paymentId}")]
        public async Task<IActionResult> GetPaymentStatus(string paymentId)
        {
            try
            {
                var pendingPayments = await _vnPayService.GetPendingPaymentsAsync("");
                var payment = pendingPayments.FirstOrDefault(p => p.PaymentId == paymentId);
                
                if (payment == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy thanh toán",
                        data = (object?)null
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy trạng thái thanh toán thành công",
                    data = payment
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Lỗi khi lấy trạng thái thanh toán: {ex.Message}",
                    data = (object?)null
                });
            }
        }

        // Helper method để extract PaymentId từ unique TxnRef
        private string ExtractPaymentIdFromTxnRef(string txnRef)
        {
            try
            {
                // TxnRef format: {PaymentId}_{yyyyMMddHHmmss}
                if (string.IsNullOrEmpty(txnRef))
                    return "";

                var lastUnderscoreIndex = txnRef.LastIndexOf('_');
                if (lastUnderscoreIndex > 0)
                {
                    return txnRef.Substring(0, lastUnderscoreIndex);
                }

                // Fallback: if no underscore found, return the whole string
                return txnRef;
            }
            catch
            {
                return "";
            }
        }
    }
}
