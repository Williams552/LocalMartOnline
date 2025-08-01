namespace LocalMartOnline.Models.DTOs.ProxyShopping
{
    public class DetailedProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public int MinimumQuantity { get; set; }
        
        // Thông tin chi tiết từ Product
        public string? Description { get; set; }
        public int PurchaseCount { get; set; }
        public double Rating { get; set; }
        public List<string> Images { get; set; } = new();
        
        // Thông tin cửa hàng
        public string? StoreId { get; set; }
        public string? StoreName { get; set; }
        public string? StoreAddress { get; set; }
        public double StoreRating { get; set; }
        public int StoreProductCount { get; set; }
        
        // Thông tin seller
        public string? SellerName { get; set; }
        public string? SellerPhone { get; set; }
    }
}
