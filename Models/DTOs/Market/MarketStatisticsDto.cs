using System;
using System.Collections.Generic;

namespace LocalMartOnline.Models.DTOs.Market
{
    public class MarketStatisticsDto
    {
        public int TotalMarkets { get; set; }
        public int ActiveMarkets { get; set; }
        public int ClosedMarkets { get; set; }

        public List<MarketDetailStatisticsDto> MarketStatistics { get; set; } = new List<MarketDetailStatisticsDto>();
    }

    public class MarketDetailStatisticsDto
    {
        public string MarketId { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int StoreCount { get; set; }
        public int SellerCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageStoreRevenue { get; set; }
        public double AverageOrdersPerStore { get; set; }
        public List<MarketStoreDto> MarketStores { get; set; } = new List<MarketStoreDto>();
        public List<MarketOrderDto> MarketOrders { get; set; } = new List<MarketOrderDto>();
    }

    public class MarketStoreDto
    {
        public string StoreId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class MarketOrderDto
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string SellerId { get; set; } = string.Empty;
    }
}