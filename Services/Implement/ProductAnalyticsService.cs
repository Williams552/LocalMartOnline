using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace LocalMartOnline.Services.Implement
{
    public class ProductAnalyticsService : IProductAnalyticsService
    {
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<OrderItem> _orderItemRepo;
        private readonly IRepository<ProxyShoppingOrder> _proxyOrderRepo;

        public ProductAnalyticsService(
            IRepository<Product> productRepo,
            IRepository<Order> orderRepo,
            IRepository<OrderItem> orderItemRepo,
            IRepository<ProxyShoppingOrder> proxyOrderRepo)
        {
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _proxyOrderRepo = proxyOrderRepo;
        }

        /// <summary>
        /// Tăng PurchaseCount khi đơn hàng thường hoàn thành
        /// </summary>
        public async Task IncrementPurchaseCountForOrderAsync(string orderId)
        {
            var orderItems = await _orderItemRepo.FindManyAsync(oi => oi.OrderId == orderId);
            var productIds = orderItems.Select(oi => oi.ProductId).Distinct().ToList();

            foreach (var productId in productIds)
            {
                var product = await _productRepo.FindOneAsync(p => p.Id == productId);
                if (product != null)
                {
                    product.PurchaseCount += 1; // Tăng 1 lần mua
                    await _productRepo.UpdateAsync(product.Id!, product);
                }
            }
        }

        /// <summary>
        /// Tăng PurchaseCount khi đơn hàng proxy shopping hoàn thành
        /// </summary>
        public async Task IncrementPurchaseCountForProxyOrderAsync(string proxyOrderId)
        {
            var proxyOrder = await _proxyOrderRepo.FindOneAsync(po => po.Id == proxyOrderId);
            if (proxyOrder?.Items == null) return;

            var productIds = proxyOrder.Items.Select(item => item.Id).Distinct().ToList();

            foreach (var productId in productIds)
            {
                if (string.IsNullOrEmpty(productId)) continue;
                var product = await _productRepo.FindOneAsync(p => p.Id == productId);
                if (product != null)
                {
                    product.PurchaseCount += 1; // Tăng 1 lần mua
                    await _productRepo.UpdateAsync(product.Id!, product);
                }
            }
        }
    }
}
