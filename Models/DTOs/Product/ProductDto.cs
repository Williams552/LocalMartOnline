using System;
using System.Collections.Generic;

namespace LocalMartOnline.Models.DTOs.Product
{
    public class ProductDto
    {
        public string? Id { get; set; }
        public string StoreId { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string UnitId { get; set; } = string.Empty;
        public decimal MinimumQuantity { get; set; } = 1;
        public ProductStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> ImageUrls { get; set; } = new();

        // Display properties
        public string StatusDisplay => Status switch
        {
            ProductStatus.Active => "Còn hàng",
            ProductStatus.OutOfStock => "Hết hàng",
            ProductStatus.Inactive => "Đã xóa",
            _ => "Không xác định"
        };
    }
}