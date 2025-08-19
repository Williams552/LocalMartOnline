namespace LocalMartOnline.Models.DTOs.Report
{
    public class ProductStatisticsDto
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int SuspendedProducts { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public List<CategoryProductStatsDto> CategoryBreakdown { get; set; } = new();
        public List<BestSellingProductDto> BestSellingProducts { get; set; } = new();
        public List<PriceRangeStatsDto> PriceRangeDistribution { get; set; } = new();
        public List<StoreProductStatsDto> TopStoresByProducts { get; set; } = new();
        public string Period { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }

    public class CategoryProductStatsDto
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int ActiveProducts { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class BestSellingProductDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int PurchaseCount { get; set; }
        public int Rank { get; set; }
        public string? PrimaryImageUrl { get; set; }
    }

    public class PriceRangeStatsDto
    {
        public string RangeName { get; set; } = string.Empty;
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int ProductCount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class StoreProductStatsDto
    {
        public string StoreId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int ActiveProducts { get; set; }
        public decimal AveragePrice { get; set; }
        public int Rank { get; set; }
    }
}