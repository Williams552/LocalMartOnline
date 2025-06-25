using System;

namespace LocalMartOnline.Models
{
    public class MarketFee
    {
        public string Id { get; set; } = string.Empty;
        public string MarketId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
