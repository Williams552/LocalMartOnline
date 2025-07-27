using System.Threading.Tasks;
using System.Collections.Generic;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Repositories;
using LocalMartOnline.Models;
using MongoDB.Driver;
using System.Linq;

namespace LocalMartOnline.Services
{
    public class SellerAnalyticsService : ISellerAnalyticsService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Review> _reviewRepository;

        public SellerAnalyticsService(
            IRepository<Order> orderRepository,
            IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IRepository<Store> storeRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Review> reviewRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _storeRepository = storeRepository;
            _orderItemRepository = orderItemRepository;
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

        Task<RevenueAnalyticsDto> ISellerAnalyticsService.GetRevenueAsync(string sellerId, string period)
            => GetRevenueAsync(sellerId, period);

        Task<OrderAnalyticsDto> ISellerAnalyticsService.GetOrderStatsAsync(string sellerId, string period)
            => GetOrderStatsAsync(sellerId, period);

        Task<List<CategoryAnalyticsDto>> ISellerAnalyticsService.GetCategoryStatsAsync(string sellerId, string period)
            => GetCategoryStatsAsync(sellerId, period);

        Task<List<ProductAnalyticsDto>> ISellerAnalyticsService.GetProductStatsAsync(string sellerId, string period)
            => GetProductStatsAsync(sellerId, period);

        public async Task<RevenueAnalyticsDto> GetRevenueAsync(string sellerId, string period)
        {
            var periodStart = GetPeriodStart(period);
            var orders = await _orderRepository.FindManyAsync(o =>
                o.SellerId == sellerId &&
                o.Status == Models.OrderStatus.Completed &&
                o.CreatedAt >= periodStart
            );
            var orderList = orders.ToList();
            decimal totalRevenue = orderList.Sum(o => o.TotalAmount);
            int orderCount = orderList.Count;
            return new RevenueAnalyticsDto
            {
                TotalRevenue = totalRevenue,
                OrderCount = orderCount,
                Period = period
            };
        }

        public async Task<OrderAnalyticsDto> GetOrderStatsAsync(string sellerId, string period)
        {
            var periodStart = GetPeriodStart(period);
            var orders = await _orderRepository.FindManyAsync(o =>
                o.SellerId == sellerId &&
                o.CreatedAt >= periodStart
            );
            var orderList = orders.ToList();
            int totalOrders = orderList.Count;
            int completedOrders = orderList.Count(o => o.Status == Models.OrderStatus.Completed);
            int cancelledOrders = orderList.Count(o => o.Status == Models.OrderStatus.Cancelled);
            return new OrderAnalyticsDto
            {
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                Period = period
            };
        }

        public async Task<List<CategoryAnalyticsDto>> GetCategoryStatsAsync(string sellerId, string period)
        {
            var periodStart = GetPeriodStart(period);
            var stores = (await _storeRepository.FindManyAsync(s => s.SellerId == sellerId)).ToList();
            var storeIds = stores.Select(s => s.Id).ToHashSet();
            var products = (await _productRepository.GetAllAsync()).Where(p => storeIds.Contains(p.StoreId)).ToList();
            var productIds = products.Select(p => p.Id).ToHashSet();
            var categories = (await _categoryRepository.GetAllAsync()).ToList();

            // Lấy các order item thuộc các sản phẩm của seller, trong các đơn hàng hoàn thành, trong khoảng thời gian
            var orderItems = (await _orderItemRepository.FindManyAsync(oi =>
                productIds.Contains(oi.ProductId) &&
                oi.PriceAtPurchase > 0 // Đảm bảo có giá
            )).ToList();

            // Lấy các order hoàn thành trong period
            var completedOrderIds = (await _orderRepository.FindManyAsync(o =>
                o.SellerId == sellerId &&
                o.Status == Models.OrderStatus.Completed &&
                o.CreatedAt >= periodStart
            )).Select(o => o.Id).ToHashSet();

            // Lọc orderItems theo các order hoàn thành
            var filteredOrderItems = orderItems.Where(oi => completedOrderIds.Contains(oi.OrderId)).ToList();

            var categoryStats = products
                .GroupBy(p => p.CategoryId)
                .Select(g =>
                {
                    var category = categories.FirstOrDefault(c => c.Id == g.Key);
                    var productIdsInCategory = g.Select(p => p.Id).ToHashSet();
                    var revenue = filteredOrderItems
                        .Where(oi => productIdsInCategory.Contains(oi.ProductId))
                        .Sum(oi => oi.Quantity * oi.PriceAtPurchase);
                    return new CategoryAnalyticsDto
                    {
                        CategoryId = g.Key,
                        CategoryName = category?.Name ?? "Unknown",
                        ProductCount = g.Count(),
                        Revenue = revenue
                    };
                })
                .ToList();
            return categoryStats;
        }

        public async Task<List<ProductAnalyticsDto>> GetProductStatsAsync(string sellerId, string period)
        {
            var periodStart = GetPeriodStart(period);
            var stores = (await _storeRepository.FindManyAsync(s => s.SellerId == sellerId)).ToList();
            var storeIds = stores.Select(s => s.Id).ToHashSet();
            var products = (await _productRepository.GetAllAsync()).Where(p => storeIds.Contains(p.StoreId)).ToList();
            var productIds = products.Select(p => p.Id).ToHashSet();

            // Lấy order items của các sản phẩm seller trong các đơn hàng hoàn thành, trong period
            var orderItems = (await _orderItemRepository.FindManyAsync(oi =>
                productIds.Contains(oi.ProductId) &&
                oi.PriceAtPurchase > 0
            )).ToList();

            var completedOrderIds = (await _orderRepository.FindManyAsync(o =>
                o.SellerId == sellerId &&
                o.Status == Models.OrderStatus.Completed &&
                o.CreatedAt >= periodStart
            )).Select(o => o.Id).ToHashSet();

            var filteredOrderItems = orderItems.Where(oi => completedOrderIds.Contains(oi.OrderId)).ToList();

            // Lấy review cho các sản phẩm
            var reviews = (await _reviewRepository.FindManyAsync(r =>
                r.TargetType == "Product" && productIds.Contains(r.TargetId)
            )).ToList();

            var productStats = products.Select(p =>
            {
                var items = filteredOrderItems.Where(oi => oi.ProductId == p.Id).ToList();
                var soldQuantity = items.Sum(oi => oi.Quantity);
                var revenue = items.Sum(oi => oi.Quantity * oi.PriceAtPurchase);
                var orderCount = items.Select(oi => oi.OrderId).Distinct().Count();
                var productReviews = reviews.Where(r => r.TargetId == p.Id).ToList();
                var avgRating = productReviews.Count > 0 ? productReviews.Average(r => r.Rating) : 0;
                return new ProductAnalyticsDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    SoldQuantity = soldQuantity,
                    Revenue = revenue,
                    OrderCount = orderCount,
                    AverageRating = avgRating
                };
            }).ToList();
            return productStats;
        }
    }
}
