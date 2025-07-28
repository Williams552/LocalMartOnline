namespace LocalMartOnline.Models.DTOs.LoyalCustomer
{
    public class LoyalCustomerDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime FirstOrderDate { get; set; }
        public DateTime LastOrderDate { get; set; }
        public int DaysSinceFirstOrder { get; set; }
        public int DaysSinceLastOrder { get; set; }
        public decimal LoyaltyScore { get; set; } // Score based on total orders, total spent, frequency
        public string CustomerTier { get; set; } = string.Empty; // Bronze, Silver, Gold, Platinum
    }

    public class GetLoyalCustomersRequestDto
    {
        public int MinimumOrders { get; set; } = 5; // Minimum number of completed orders to be considered loyal
        public decimal MinimumSpent { get; set; } = 0; // Minimum total amount spent
        public int DaysRange { get; set; } = 365; // Consider orders within last X days
        public string SortBy { get; set; } = "LoyaltyScore"; // LoyaltyScore, TotalOrders, TotalSpent, LastOrderDate
        public string SortOrder { get; set; } = "Desc"; // Asc, Desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class GetLoyalCustomersResponseDto
    {
        public List<LoyalCustomerDto> Customers { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public LoyalCustomerStatisticsDto Statistics { get; set; } = new();
    }

    public class LoyalCustomerStatisticsDto
    {
        public int TotalLoyalCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageCustomerValue { get; set; }
        public int BronzeCustomers { get; set; }
        public int SilverCustomers { get; set; }
        public int GoldCustomers { get; set; }
        public int PlatinumCustomers { get; set; }
        public decimal RepeatCustomerRate { get; set; } // Percentage of customers with more than 1 order
    }

    public class CustomerOrderSummaryDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<OrderSummaryDto> RecentOrders { get; set; } = new();
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class OrderSummaryDto
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; } = 0;
    }
}
