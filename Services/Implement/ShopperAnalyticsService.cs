using System.Threading.Tasks;
using System.Linq;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Models.DTOs.Shopper;
using LocalMartOnline.Repositories;
using LocalMartOnline.Models;
using System.Collections.Generic;

namespace LocalMartOnline.Services
{
    public class ShopperAnalyticsService : IShopperAnalyticsService
    {
        private readonly IRepository<ProxyShoppingOrder> _orderRepository;
        private readonly IRepository<Review> _reviewRepository;

        public ShopperAnalyticsService(
            IRepository<ProxyShoppingOrder> orderRepository,
            IRepository<Review> reviewRepository)
        {
            _orderRepository = orderRepository;
            _reviewRepository = reviewRepository;
        }

        private DateTime GetPeriodStart(string period)
        {
            if (string.IsNullOrEmpty(period)) return DateTime.MinValue;
            if (period.EndsWith("d") && int.TryParse(period.TrimEnd('d'), out int days))
                return DateTime.UtcNow.AddDays(-days);
            if (period.EndsWith("m") && int.TryParse(period.TrimEnd('m'), out int months))
                return DateTime.UtcNow.AddMonths(-months);
            return DateTime.MinValue;
        }

        public async Task<ShopperAnalyticsDto> GetShopperAnalyticsAsync(string shopperId, string period)
        {
            var periodStart = GetPeriodStart(period);

            var orders = await _orderRepository.FindManyAsync(o =>
                o.ProxyShopperId == shopperId &&
                o.CreatedAt >= periodStart
            );
            var orderList = orders.ToList();

            int totalOrders = orderList.Count;
            int completedOrders = orderList.Count(o => o.Status == ProxyOrderStatus.Completed);
            decimal totalSpent = orderList
                .Where(o => o.Status == ProxyOrderStatus.Completed)
                .Sum(o => o.TotalAmount ?? 0);

            var reviews = await _reviewRepository.FindManyAsync(r =>
                r.UserId == shopperId &&
                r.CreatedAt >= periodStart
            );
            var reviewList = reviews.ToList();
            double averageRating = reviewList.Count > 0
                ? reviewList.Average(r => r.Rating)
                : 0;

            return new ShopperAnalyticsDto
            {
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                TotalSpent = totalSpent,
                AverageRating = averageRating
            };
        }
    }
}