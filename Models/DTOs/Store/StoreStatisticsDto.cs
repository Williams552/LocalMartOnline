using System;
using System.Collections.Generic;

namespace LocalMartOnline.Models.DTOs.Store
{
    public class StoreStatisticsDto
    {
        public Dictionary<string, string> StoreNames { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> MarketNames { get; set; } = new Dictionary<string, string>();

        // Basic store metrics
        public int TotalStoreCount { get; set; }
        public Dictionary<string, int> StoresByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> StoresByMarket { get; set; } = new Dictionary<string, int>();
        public double AverageStoreRating { get; set; }

        // Store performance tiers
        public Dictionary<string, int> StorePerformanceTiers { get; set; } = new Dictionary<string, int>();

        // Store analytics
        public Dictionary<string, decimal> StorePerformanceRanking { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, int> MarketDistribution { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> RevenuePerStore { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, int> ProductCatalogSize { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> CustomerEngagement { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> GrowthTrends { get; set; } = new Dictionary<string, decimal>();

        // Time period information
        public DateTime PeriodStart { get; set; } = DateTime.UtcNow;
        public DateTime PeriodEnd { get; set; } = DateTime.UtcNow;
        public string? MarketId { get; set; }
    }
}