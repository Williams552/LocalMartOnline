using System;
using System.Collections.Generic;
using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Models.DTOs.ProxyShopping
{
    public class ProxyShoppingProposalDTO
    {
        public string? OrderId { get; set; }
        public string? ProxyShopperId { get; set; }
        public List<ProductDto> Items { get; set; } = new();
        public decimal TotalProductPrice { get; set; }
        public decimal ProxyFee { get; set; }
        public decimal TotalAmount => TotalProductPrice + ProxyFee;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
