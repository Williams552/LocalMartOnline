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

        public SellerAnalyticsService(
            IRepository<Order> orderRepository,
            IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IRepository<Store> storeRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _storeRepository = storeRepository;
        }

        Task<RevenueAnalyticsDto> ISellerAnalyticsService.GetRevenueAsync(string sellerId, string period)
        {
            return GetRevenueAsync(sellerId, period);
        }

        Task<OrderAnalyticsDto> ISellerAnalyticsService.GetOrderStatsAsync(string sellerId, string period)
        {
            return GetOrderStatsAsync(sellerId, period);
        }

        Task<List<CategoryAnalyticsDto>> ISellerAnalyticsService.GetCategoryStatsAsync(string sellerId, string period)
        {
            return GetCategoryStatsAsync(sellerId, period);
        }

        Task<List<ProductAnalyticsDto>> ISellerAnalyticsService.GetProductStatsAsync(string sellerId, string period)
        {
            return GetProductStatsAsync(sellerId, period);
        }

        public async Task<RevenueAnalyticsDto> GetRevenueAsync(string sellerId, string period)
        {
            var orders = await _orderRepository.FindManyAsync(o => o.SellerId == sellerId && o.Status == Models.OrderStatus.Completed);
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
            var orders = await _orderRepository.FindManyAsync(o => o.SellerId == sellerId);
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
            var stores = (await _storeRepository.FindManyAsync(s => s.SellerId == sellerId)).ToList();
            var storeIds = stores.Select(s => s.Id).ToHashSet();
            var products = (await _productRepository.GetAllAsync()).Where(p => storeIds.Contains(p.StoreId)).ToList();
            var categories = (await _categoryRepository.GetAllAsync()).ToList();
            var orders = (await _orderRepository.FindManyAsync(o => o.SellerId == sellerId && o.Status == Models.OrderStatus.Completed)).ToList();

            // NOTE: Order does not have Items property, so revenue per category cannot be calculated without order items
            var categoryStats = products
                .GroupBy(p => p.CategoryId)
                .Select(g =>
                {
                    var category = categories.FirstOrDefault(c => c.Id == g.Key);
                    return new CategoryAnalyticsDto
                    {
                        CategoryId = g.Key,
                        CategoryName = category?.Name ?? "Unknown",
                        ProductCount = g.Count(),
                        Revenue = 0 // Cannot calculate without order items
                    };
                })
                .ToList();
            return categoryStats;
        }

        public async Task<List<ProductAnalyticsDto>> GetProductStatsAsync(string sellerId, string period)
        {
            var stores = (await _storeRepository.FindManyAsync(s => s.SellerId == sellerId)).ToList();
            var storeIds = stores.Select(s => s.Id).ToHashSet();
            var products = (await _productRepository.GetAllAsync()).Where(p => storeIds.Contains(p.StoreId)).ToList();
            // NOTE: Order does not have Items property, so cannot calculate sold quantity or revenue per product
            var productStats = products.Select(p =>
            {
                return new ProductAnalyticsDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    SoldQuantity = 0, // Cannot calculate without order items
                    Revenue = 0
                };
            }).ToList();
            return productStats;
        }
    }
}
