using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IMarketFeePaymentService
    {
        Task<IEnumerable<MarketFeePaymentDto>> GetPaymentsBySellerAsync(long sellerId);
        Task<MarketFeePaymentDto?> GetPaymentByIdAsync(long paymentId);
        Task<MarketFeePaymentDto> CreatePaymentAsync(MarketFeePaymentCreateDto dto);
        Task<bool> UpdatePaymentStatusAsync(long paymentId, string status);
    }
}
