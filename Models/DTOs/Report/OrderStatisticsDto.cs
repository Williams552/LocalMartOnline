namespace LocalMartOnline.Models.DTOs.Report
{
    public class OrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int PaidOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal OrderCompletionRate { get; set; }
        public int UniqueCustomers { get; set; }
        public decimal RevenueGrowth { get; set; }
        public decimal OrderGrowth { get; set; }
        public List<OrderStatusStatsDto> OrdersByStatus { get; set; } = new();
        public List<OrderTrendDto> OrderTrends { get; set; } = new();
        public List<TopCustomerDto> TopCustomers { get; set; } = new();
        public List<TopSellerDto> TopSellers { get; set; } = new();
        public List<HourlyOrderStatsDto> PeakOrderingHours { get; set; } = new();
        public OrderMetricsDto OrderMetrics { get; set; } = new();
        public CustomerPatternsDto CustomerPatterns { get; set; } = new();
        public string Period { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }

    public class OrderStatusStatsDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AverageValue { get; set; }
    }

    public class OrderTrendDto
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal CompletionRate { get; set; }
        public int CumulativeOrders { get; set; }
    }

    public class TopCustomerDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime LastOrderDate { get; set; }
        public int Rank { get; set; }
    }

    public class TopSellerDto
    {
        public string SellerId { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal CompletionRate { get; set; }
        public int Rank { get; set; }
    }

    public class HourlyOrderStatsDto
    {
        public int Hour { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
    }

    public class OrderMetricsDto
    {
        public int TotalItems { get; set; }
        public decimal AverageItemPrice { get; set; }
        public decimal AverageItemsPerOrder { get; set; }
        public TimeSpan AverageProcessingTime { get; set; }
        public decimal RefundRate { get; set; }
        public decimal CancellationRate { get; set; }
        public WeekComparisonDto WeekdayVsWeekend { get; set; } = new();
    }

    public class CustomerPatternsDto
    {
        public int TotalUniqueCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ReturningCustomers { get; set; }
        public decimal AverageOrdersPerCustomer { get; set; }
        public decimal CustomerRetentionRate { get; set; }
        public decimal RepeatCustomerRate { get; set; }
    }

    public class WeekComparisonDto
    {
        public int WeekdayOrders { get; set; }
        public int WeekendOrders { get; set; }
        public decimal WeekdayRevenue { get; set; }
        public decimal WeekendRevenue { get; set; }
        public decimal WeekdayAvgOrder { get; set; }
        public decimal WeekendAvgOrder { get; set; }
    }
}