using LocalMartOnline.Models.DTOs.LoyalCustomer;

namespace LocalMartOnline.Services.Interface
{
    public interface ICustomerService
    {
        Task<GetLoyalCustomersResponseDto> GetLoyalCustomersAsync(string sellerId, GetLoyalCustomersRequestDto request);
        Task<CustomerOrderSummaryDto?> GetCustomerOrderSummaryAsync(string sellerId, string customerId);
        Task<LoyalCustomerStatisticsDto> GetLoyalCustomerStatisticsAsync(string sellerId);
        decimal CalculateLoyaltyScore(int totalOrders, decimal totalSpent, int daysSinceFirstOrder, int daysSinceLastOrder);
        string DetermineCustomerTier(decimal loyaltyScore);
    }
}
