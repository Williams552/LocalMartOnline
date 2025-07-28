namespace LocalMartOnline.Models.DTOs.Order
{
    public class OrderItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public string ProductUnitName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }
}