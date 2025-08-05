using System;
using System.Collections.Generic;
using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Models.DTOs.ProxyShopping
{
    public class AdminProxyRequestDto
    {
        public string Id { get; set; } = string.Empty;
        public string? ProxyOrderId { get; set; } = string.Empty;
        public string BuyerId { get; set; } = string.Empty;
        public string? BuyerName { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerPhone { get; set; }
        public string? ProxyShopperId { get; set; }
        public string? ProxyShopperName { get; set; }
        public string? ProxyShopperEmail { get; set; }
        public string? ProxyShopperPhone { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public string? OrderStatus { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ProxyItem> Items { get; set; } = new();
        public string? DeliveryAddress { get; set; }
    }
}
