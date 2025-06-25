using MongoDB.Driver;
using LocalMartOnline.Models;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Models.DTOs.LoyalCustomer;

namespace LocalMartOnline.Services.Implement
{
    public class CustomerService : ICustomerService
    {
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly IMongoCollection<OrderItem> _orderItemCollection;
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<Store> _storeCollection;
        private readonly IMongoCollection<User> _userCollection;

        public CustomerService(IMongoDatabase database)
        {
            _orderCollection = database.GetCollection<Order>("orders");
            _orderItemCollection = database.GetCollection<OrderItem>("order_items");
            _productCollection = database.GetCollection<Product>("products");
            _storeCollection = database.GetCollection<Store>("stores");
            _userCollection = database.GetCollection<User>("users");
        }

        public async Task<GetLoyalCustomersResponseDto> GetLoyalCustomersAsync(string sellerId, GetLoyalCustomersRequestDto request)
        {
            // Get seller's store
            var store = await _storeCollection
                .Find(Builders<Store>.Filter.Eq(s => s.SellerId, sellerId))
                .FirstOrDefaultAsync();

            if (store == null)
                return new GetLoyalCustomersResponseDto();

            // Get seller's products
            var sellerProducts = await _productCollection
                .Find(Builders<Product>.Filter.Eq(p => p.StoreId, store.Id))
                .ToListAsync();

            var productIds = sellerProducts.Select(p => p.Id).ToList();

            // Date range filter
            var fromDate = DateTime.UtcNow.AddDays(-request.DaysRange);

            // Get orders containing seller's products
            var orderItems = await _orderItemCollection
                .Find(Builders<OrderItem>.Filter.In(oi => oi.ProductId, productIds))
                .ToListAsync();

            var orderIds = orderItems.Select(oi => oi.OrderId).Distinct().ToList();

            // Get completed orders
            var orders = await _orderCollection
                .Find(Builders<Order>.Filter.And(
                    Builders<Order>.Filter.In(o => o.Id, orderIds),
                    Builders<Order>.Filter.Eq(o => o.Status, "Completed"),
                    Builders<Order>.Filter.Gte(o => o.CreatedAt, fromDate)
                ))
                .ToListAsync();

            // Group orders by buyer and calculate statistics
            var customerStats = new Dictionary<string, CustomerStats>();

            foreach (var order in orders)
            {
                if (!customerStats.ContainsKey(order.BuyerId))
                {
                    customerStats[order.BuyerId] = new CustomerStats
                    {
                        BuyerId = order.BuyerId,
                        FirstOrderDate = order.CreatedAt,
                        LastOrderDate = order.CreatedAt
                    };
                }

                var stats = customerStats[order.BuyerId];
                stats.TotalOrders++;
                
                // Calculate amount for seller's products only
                var sellerOrderItems = orderItems.Where(oi => oi.OrderId == order.Id).ToList();
                var sellerAmount = sellerOrderItems.Sum(oi => oi.PriceAtPurchase * oi.Quantity);
                stats.TotalSpent += sellerAmount;

                if (order.CreatedAt < stats.FirstOrderDate)
                    stats.FirstOrderDate = order.CreatedAt;
                if (order.CreatedAt > stats.LastOrderDate)
                    stats.LastOrderDate = order.CreatedAt;
            }

            // Filter by minimum criteria
            var loyalCustomers = customerStats.Values
                .Where(cs => cs.TotalOrders >= request.MinimumOrders && cs.TotalSpent >= request.MinimumSpent)
                .ToList();

            // Get user details and calculate loyalty metrics
            var customerDtos = new List<LoyalCustomerDto>();

            foreach (var stats in loyalCustomers)
            {
                var user = await _userCollection
                    .Find(Builders<User>.Filter.Eq(u => u.Id, stats.BuyerId))
                    .FirstOrDefaultAsync();

                if (user == null) continue;

                var daysSinceFirstOrder = (DateTime.UtcNow - stats.FirstOrderDate).Days;
                var daysSinceLastOrder = (DateTime.UtcNow - stats.LastOrderDate).Days;
                var loyaltyScore = CalculateLoyaltyScore(stats.TotalOrders, stats.TotalSpent, daysSinceFirstOrder, daysSinceLastOrder);

                customerDtos.Add(new LoyalCustomerDto
                {
                    UserId = user.Id!,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    TotalOrders = stats.TotalOrders,
                    CompletedOrders = stats.TotalOrders, // All orders we counted are completed
                    TotalSpent = stats.TotalSpent,
                    AverageOrderValue = stats.TotalOrders > 0 ? stats.TotalSpent / stats.TotalOrders : 0,
                    FirstOrderDate = stats.FirstOrderDate,
                    LastOrderDate = stats.LastOrderDate,
                    DaysSinceFirstOrder = daysSinceFirstOrder,
                    DaysSinceLastOrder = daysSinceLastOrder,
                    LoyaltyScore = loyaltyScore,
                    CustomerTier = DetermineCustomerTier(loyaltyScore)
                });
            }

            // Sort results
            customerDtos = request.SortBy.ToLower() switch
            {
                "totalorders" => request.SortOrder.ToLower() == "asc" 
                    ? customerDtos.OrderBy(c => c.TotalOrders).ToList()
                    : customerDtos.OrderByDescending(c => c.TotalOrders).ToList(),
                "totalspent" => request.SortOrder.ToLower() == "asc"
                    ? customerDtos.OrderBy(c => c.TotalSpent).ToList()
                    : customerDtos.OrderByDescending(c => c.TotalSpent).ToList(),
                "lastorderdate" => request.SortOrder.ToLower() == "asc"
                    ? customerDtos.OrderBy(c => c.LastOrderDate).ToList()
                    : customerDtos.OrderByDescending(c => c.LastOrderDate).ToList(),
                _ => request.SortOrder.ToLower() == "asc"
                    ? customerDtos.OrderBy(c => c.LoyaltyScore).ToList()
                    : customerDtos.OrderByDescending(c => c.LoyaltyScore).ToList()
            };

            // Pagination
            var totalCount = customerDtos.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var pagedCustomers = customerDtos
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Calculate statistics
            var statistics = await GetLoyalCustomerStatisticsAsync(sellerId);

            return new GetLoyalCustomersResponseDto
            {
                Customers = pagedCustomers,
                TotalCount = totalCount,
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                Statistics = statistics
            };
        }

        public async Task<CustomerOrderSummaryDto?> GetCustomerOrderSummaryAsync(string sellerId, string customerId)
        {
            // Get seller's store
            var store = await _storeCollection
                .Find(Builders<Store>.Filter.Eq(s => s.SellerId, sellerId))
                .FirstOrDefaultAsync();

            if (store == null) return null;

            // Get customer details
            var customer = await _userCollection
                .Find(Builders<User>.Filter.Eq(u => u.Id, customerId))
                .FirstOrDefaultAsync();

            if (customer == null) return null;

            // Get seller's products
            var sellerProducts = await _productCollection
                .Find(Builders<Product>.Filter.Eq(p => p.StoreId, store.Id))
                .ToListAsync();

            var productIds = sellerProducts.Select(p => p.Id).ToList();

            // Get order items for seller's products
            var orderItems = await _orderItemCollection
                .Find(Builders<OrderItem>.Filter.In(oi => oi.ProductId, productIds))
                .ToListAsync();

            var orderIds = orderItems.Select(oi => oi.OrderId).Distinct().ToList();

            // Get customer's orders containing seller's products
            var orders = await _orderCollection
                .Find(Builders<Order>.Filter.And(
                    Builders<Order>.Filter.Eq(o => o.BuyerId, customerId),
                    Builders<Order>.Filter.In(o => o.Id, orderIds)
                ))
                .SortByDescending(o => o.CreatedAt)
                .ToListAsync();

            var recentOrders = new List<OrderSummaryDto>();
            decimal totalSpent = 0;

            foreach (var order in orders.Take(10)) // Last 10 orders
            {
                var sellerOrderItems = orderItems.Where(oi => oi.OrderId == order.Id).ToList();
                var orderAmount = sellerOrderItems.Sum(oi => oi.PriceAtPurchase * oi.Quantity);
                totalSpent += orderAmount;

                recentOrders.Add(new OrderSummaryDto
                {
                    OrderId = order.Id!,
                    TotalAmount = orderAmount,
                    Status = order.Status,
                    CreatedAt = order.CreatedAt,
                    ItemCount = sellerOrderItems.Sum(oi => oi.Quantity)
                });
            }

            // Calculate total for all orders
            foreach (var order in orders.Skip(10))
            {
                var sellerOrderItems = orderItems.Where(oi => oi.OrderId == order.Id).ToList();
                totalSpent += sellerOrderItems.Sum(oi => oi.PriceAtPurchase * oi.Quantity);
            }

            return new CustomerOrderSummaryDto
            {
                CustomerId = customer.Id!,
                CustomerName = customer.FullName,
                Email = customer.Email,
                RecentOrders = recentOrders,
                TotalOrders = orders.Count,
                TotalSpent = totalSpent
            };
        }

        public async Task<LoyalCustomerStatisticsDto> GetLoyalCustomerStatisticsAsync(string sellerId)
        {
            var request = new GetLoyalCustomersRequestDto
            {
                MinimumOrders = 1, // Get all customers
                MinimumSpent = 0,
                DaysRange = 365,
                Page = 1,
                PageSize = int.MaxValue
            };

            var allCustomers = await GetLoyalCustomersAsync(sellerId, request);
            var loyalCustomers = allCustomers.Customers.Where(c => c.TotalOrders >= 5).ToList(); // Loyal = 5+ orders

            var statistics = new LoyalCustomerStatisticsDto
            {
                TotalLoyalCustomers = loyalCustomers.Count,
                TotalRevenue = loyalCustomers.Sum(c => c.TotalSpent),
                AverageCustomerValue = loyalCustomers.Any() ? loyalCustomers.Average(c => c.TotalSpent) : 0,
                BronzeCustomers = loyalCustomers.Count(c => c.CustomerTier == "Bronze"),
                SilverCustomers = loyalCustomers.Count(c => c.CustomerTier == "Silver"),
                GoldCustomers = loyalCustomers.Count(c => c.CustomerTier == "Gold"),
                PlatinumCustomers = loyalCustomers.Count(c => c.CustomerTier == "Platinum"),
                RepeatCustomerRate = allCustomers.Customers.Any() 
                    ? (decimal)allCustomers.Customers.Count(c => c.TotalOrders > 1) / allCustomers.Customers.Count * 100 
                    : 0
            };

            return statistics;
        }

        public decimal CalculateLoyaltyScore(int totalOrders, decimal totalSpent, int daysSinceFirstOrder, int daysSinceLastOrder)
        {
            // Base score from orders and spending
            decimal orderScore = Math.Min(totalOrders * 10, 500); // Max 500 points from orders
            decimal spendingScore = Math.Min(totalSpent / 10, 300); // Max 300 points from spending

            // Loyalty duration bonus (longer relationship = better)
            decimal durationBonus = Math.Min(daysSinceFirstOrder / 30 * 5, 100); // Max 100 points for duration

            // Recency penalty (recent activity is better)
            decimal recencyPenalty = Math.Max(daysSinceLastOrder / 7 * 2, 0); // Penalty for inactivity

            // Frequency bonus
            decimal frequencyScore = daysSinceFirstOrder > 0 
                ? Math.Min((decimal)totalOrders / daysSinceFirstOrder * 365 * 50, 100) // Max 100 points for frequency
                : 0;

            decimal finalScore = orderScore + spendingScore + durationBonus + frequencyScore - recencyPenalty;
            return Math.Max(finalScore, 0); // Ensure non-negative score
        }

        public string DetermineCustomerTier(decimal loyaltyScore)
        {
            return loyaltyScore switch
            {
                >= 800 => "Platinum",
                >= 600 => "Gold",
                >= 400 => "Silver",
                _ => "Bronze"
            };
        }

        private class CustomerStats
        {
            public string BuyerId { get; set; } = string.Empty;
            public int TotalOrders { get; set; }
            public decimal TotalSpent { get; set; }
            public DateTime FirstOrderDate { get; set; }
            public DateTime LastOrderDate { get; set; }
        }
    }
}
