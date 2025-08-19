namespace LocalMartOnline.Models.DTOs.Report
{
    public class CategoryStatisticsDto
    {
        public int TotalCategories { get; set; }
        public int ActiveCategories { get; set; }
        public int InactiveCategories { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageProductsPerCategory { get; set; }
        public List<CategoryPerformanceDto> TopPerformingCategories { get; set; } = new();
        public List<CategoryDistributionDto> CategoryDistribution { get; set; } = new();
        public List<CategoryTrendDto> CategoryTrends { get; set; } = new();
        public List<CategoryMarketShareDto> MarketShareAnalysis { get; set; } = new();
        public string Period { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }

    public class CategoryPerformanceDto
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Rank { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MarketShare { get; set; }
        public decimal GrowthRate { get; set; }
        public string PerformanceTier { get; set; } = string.Empty;
    }

    public class CategoryDistributionDto
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal Percentage { get; set; }
        public decimal Revenue { get; set; }
        public decimal RevenuePercentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CategoryTrendDto
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public List<CategoryTrendDataPoint> DataPoints { get; set; } = new();
        public string TrendDirection { get; set; } = string.Empty;
        public decimal GrowthRate { get; set; }
        public string Seasonality { get; set; } = string.Empty;
    }

    public class CategoryTrendDataPoint
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class CategoryMarketShareDto
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal MarketShare { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryCompetitorDto> TopStores { get; set; } = new();
        public string Trend { get; set; } = string.Empty;
        public decimal GrowthRate { get; set; }
    }

    public class CategoryCompetitorDto
    {
        public string StoreId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int ProductCount { get; set; }
        public decimal MarketShare { get; set; }
    }
}