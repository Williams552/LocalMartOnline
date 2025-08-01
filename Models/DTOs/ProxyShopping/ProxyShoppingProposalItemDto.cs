namespace LocalMartOnline.Models.DTOs.ProxyShopping
{
    public class ProxyShoppingProposalItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
