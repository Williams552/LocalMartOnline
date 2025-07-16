using System;
using System.Collections.Generic;

namespace LocalMartOnline.Models.DTOs.Order
{
    public class OrderCreateDto
    {
        public string BuyerId { get; set; } = string.Empty;
        public string SellerId { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
        public string? Notes { get; set; }
        public DateTime? ExpectedDeliveryTime { get; set; }
    }
}