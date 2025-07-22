using System.Collections.Generic;
using LocalMartOnline.Models.DTOs.Cart;

namespace LocalMartOnline.Models.DTOs.Order
{
    public class CartOrderCreateDto
    {
        public string BuyerId { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
    }
}
