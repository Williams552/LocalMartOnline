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

        // Smart search properties
        public int PurchaseCount { get; set; } = 0; // Số lần được mua thành công
        public decimal Score { get; set; } = 0;
        public SellerDto? Seller { get; set; }

        // Bổ sung cho FE
        public string StoreName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
    }
}