using System.Collections.Generic;

namespace LocalMartOnline.Models.DTOs.Product
{
    public class ProductUpdateDto
    {
        public string CategoryId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string UnitId { get; set; } = string.Empty;
        public decimal MinimumQuantity { get; set; } = 1;
        public List<string> ImageUrls { get; set; } = new();
    }
}