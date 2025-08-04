using System;
using System.Collections.Generic;
using LocalMartOnline.Models;

namespace LocalMartOnline.Models.DTOs.ProxyShopping
{
    public class ProxyRequestResponseDto
    {
        public string? Id { get; set; }
        public string? ProxyOrderId { get; set; }
        public List<ProxyItem> Items { get; set; } = new();
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerPhone { get; set; }
        public string? MarketId { get; set; }
        public string? MarketName { get; set; }
    }
}
