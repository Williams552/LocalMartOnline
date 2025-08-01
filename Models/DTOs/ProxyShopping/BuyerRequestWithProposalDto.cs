using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Models.DTOs.ProxyShopping
{
    public class BuyerRequestWithProposalDto
    {
        public string Id { get; set; } = string.Empty;
        public List<ProxyItem> Items { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Thông tin proxy shopper (nếu có)
        public string? ProxyShopperId { get; set; }
        public string? ProxyShopperName { get; set; }
        public string? ProxyShopperPhone { get; set; }
        
        // Thông tin đề xuất (nếu có)
        public ProposalInfo? Proposal { get; set; }
    }

    public class ProposalInfo
    {
        public List<ProductDto> ProposedItems { get; set; } = new();
        public decimal? TotalAmount { get; set; }
        public decimal? ProxyFee { get; set; }
        public string? Notes { get; set; }
        public DateTime? ProposedAt { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
    }
}
