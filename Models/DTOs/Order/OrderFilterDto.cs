using LocalMartOnline.Models.DTOs.Common;
using System;

namespace LocalMartOnline.Models.DTOs.Order
{
    public class OrderFilterDto : PagedRequestDto
    {
        public string? BuyerId { get; set; }
        public string? SellerId { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}