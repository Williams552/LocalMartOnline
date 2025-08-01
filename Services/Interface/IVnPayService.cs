using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Payment;

namespace LocalMartOnline.Services;
public interface IVnPayService
{
    string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
    PaymentResponseModel PaymentExecute(IQueryCollection collections);
    
    // Methods for MarketFeePayment
    Task<IEnumerable<PendingPaymentDto>> GetPendingPaymentsAsync(string sellerId);
    Task<string> CreateMarketFeePaymentUrlAsync(string paymentId, HttpContext context);
    Task<bool> ProcessMarketFeePaymentCallbackAsync(IQueryCollection collections);
}