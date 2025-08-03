using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Payment;

namespace LocalMartOnline.Services;
public interface IVnPayService
{
    string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
    PaymentResponseModel PaymentExecute(IQueryCollection collections);
    
    // New methods for MarketFeePayment
    Task<List<PendingPaymentDto>> GetPendingPaymentsAsync(string sellerId);
    Task<CreatePaymentUrlResponseDto> CreateMarketFeePaymentUrlAsync(CreatePaymentUrlRequestDto request, HttpContext context);
    Task<bool> ProcessMarketFeePaymentCallbackAsync(IQueryCollection collections);
}