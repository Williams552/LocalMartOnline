using System;
using System.Collections.Generic;

namespace LocalMartOnline.Models.DTOs.Report
{
    public class SellerMetricsDto
    {
        public int TotalActiveSellers { get; set; }
        public Dictionary<string, int> SellersByMarket { get; set; } = new Dictionary<string, int>();
        public int NewSellerRegistrations { get; set; }
        public Dictionary<string, int> SellerActivityLevels { get; set; } = new Dictionary<string, int>();
        public double StoreToSellerRatio { get; set; }
        public Dictionary<string, int> SellerPerformanceTiers { get; set; } = new Dictionary<string, int>();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}