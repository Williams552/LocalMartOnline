
using System.Threading.Tasks;
using System.Collections.Generic;
using LocalMartOnline.Models.DTOs;

namespace LocalMartOnline.Services.Interface
{
    public interface ISellerAnalyticsService
    {
        Task<RevenueAnalyticsDto> GetRevenueAsync(string sellerId, string period);
        Task<OrderAnalyticsDto> GetOrderStatsAsync(string sellerId, string period);
        Task<List<CategoryAnalyticsDto>> GetCategoryStatsAsync(string sellerId, string period);
        Task<List<ProductAnalyticsDto>> GetProductStatsAsync(string sellerId, string period);
    }
}
