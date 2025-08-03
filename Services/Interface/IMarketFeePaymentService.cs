using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.Store;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IMarketFeePaymentService
    {
        Task<IEnumerable<MarketFeePaymentDto>> GetPaymentsBySellerAsync(string sellerId);
        Task<MarketFeePaymentDto?> GetPaymentByIdAsync(string paymentId);
        Task<MarketFeePaymentDto> CreatePaymentAsync(MarketFeePaymentCreateDto dto);
        
        // New methods for seller payment status tracking
        Task<GetSellersPaymentStatusResponseDto> GetSellersPaymentStatusAsync(GetSellersPaymentStatusRequestDto request);
        
        // New method for admin to get all payments with filtering
        Task<GetAllMarketFeePaymentsResponseDto> GetAllPaymentsAsync(GetAllMarketFeePaymentsRequestDto request);
        
        // Methods moved from StoreService
        Task<GetAllStoresWithPaymentResponseDto> GetAllStoresWithPaymentInfoAsync(GetAllStoresWithPaymentRequestDto request);
        Task<bool> UpdateStorePaymentStatusAsync(string paymentId, UpdateStorePaymentStatusDto dto);
        
        // Admin method to create payment for any user with any fee type
        Task<AdminCreatePaymentResponseDto> CreatePaymentByAdminAsync(AdminCreatePaymentDto dto);
        
        // Admin method to create payment for all sellers in a market
        Task<AdminCreatePaymentForMarketResponseDto> CreatePaymentForMarketAsync(AdminCreatePaymentForMarketDto dto);
    }
}
