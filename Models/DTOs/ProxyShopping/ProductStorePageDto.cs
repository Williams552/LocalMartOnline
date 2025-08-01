namespace LocalMartOnline.Models.DTOs.ProxyShopping
{
    public class ProductStorePageDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        
        // Thông tin cửa hàng
        public string StoreId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string StoreAddress { get; set; } = string.Empty;
        public double StoreRating { get; set; }
        public int StoreTotalOrders { get; set; }
        
        // Thông tin sản phẩm chi tiết
        public string Description { get; set; } = string.Empty;
        public int PurchaseCount { get; set; }
        public int StockQuantity { get; set; }
        public double ProductRating { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        
        // Thông tin bổ sung
        public string CategoryName { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        public DateTime UpdatedAt { get; set; }
    }
}
