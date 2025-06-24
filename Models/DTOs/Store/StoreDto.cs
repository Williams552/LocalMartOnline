using System;

namespace LocalMartOnline.Models.DTOs.Store
{
    public class StoreDto
    {
        public string? Id { get; set; }
        public string SellerId { get; set; }
        public string MarketId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
        public decimal Rating { get; set; }
        public string StoreImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}