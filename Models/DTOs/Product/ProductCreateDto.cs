using System.Collections.Generic;

namespace LocalMartOnline.Models.DTOs.Product
{
    public class ProductCreateDto
    {
        public string StoreId { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}