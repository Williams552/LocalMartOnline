using System;
using System.Collections.Generic;

namespace LocalMartOnline.Models.DTOs.Order
{
    public class OrderDto
    {
        public string? Id { get; set; }
        public string BuyerId { get; set; } = string.Empty;
        public string SellerId { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty; 
        public string BuyerPhone { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string PaymentStatus { get; set; } = "Pending";
        public DateTime? ExpectedDeliveryTime { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}