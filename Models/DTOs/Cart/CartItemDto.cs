using System;

namespace LocalMartOnline.Models.DTOs.Cart
{
    public class CartItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string CartId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ProductInCartDto Product { get; set; } = new ProductInCartDto();
        public decimal SubTotal { get; set; }
    }

    public class ProductInCartDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Images { get; set; } = string.Empty; // First image URL
        public string Unit { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public decimal StockQuantity { get; set; }
        public decimal MinimumQuantity { get; set; }
        public string StoreId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
    }

    public class CartSummaryDto
    {
        public int TotalItems { get; set; }
        public int UniqueProducts { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal EstimatedTax { get; set; }
        public decimal EstimatedTotal { get; set; }
    }

    public class AddToCartDto
    {
        public string ProductId { get; set; } = string.Empty;
        public double Quantity { get; set; }
    }

    public class UpdateCartItemDto
    {
        public double Quantity { get; set; }
    }
}
