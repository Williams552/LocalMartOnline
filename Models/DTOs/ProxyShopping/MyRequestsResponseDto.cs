using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Models.DTOs.ProxyShopping
{
    public class ProxyRequestsResponseDto
    {
        // Request Information
        public string Id { get; set; } = string.Empty;
        public string? ProxyOrderId { get; set; } = string.Empty; // Optional, if the request has an associated order
        public List<ProxyItem> Items { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? StoreId { get; set; }
        public string? StoreName { get; set; }

        // Partner Information (Buyer info for Proxy, Proxy info for Buyer)
        public string? PartnerName { get; set; }
        public string? PartnerEmail { get; set; }
        public string? PartnerPhone { get; set; }
        public string PartnerRole { get; set; } = string.Empty; // "Buyer" or "Proxy Shopper"

        // Order Information (if exists)
        public string? OrderId { get; set; }
        public string? OrderStatus { get; set; }
        public List<ProductDto>? OrderItems { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? ProxyFee { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? Notes { get; set; }
        public string? ProofImages { get; set; }
        public DateTime? OrderCreatedAt { get; set; }
        public DateTime? OrderUpdatedAt { get; set; }

        // UI Helpers
        public bool HasOrder => !string.IsNullOrEmpty(OrderId);
        public string CurrentPhase { get; set; } = string.Empty;
        // Buyer Information (for admin detail)
        public string? BuyerName { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerPhone { get; set; }
    }
}
