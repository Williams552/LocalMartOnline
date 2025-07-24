using System;
using System.Collections.Generic;

namespace LocalMartOnline.Models.DTOs.FastBargain
{
    public class FastBargainCreateRequestDTO
    {
        public string ProductId { get; set; } = string.Empty;
        public string BuyerId { get; set; } = string.Empty;
        public string SellerId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal InitialOfferPrice { get; set; }
    }

    public class FastBargainProposalDTO
    {
        public string BargainId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal ProposedPrice { get; set; }
        public DateTime ProposedAt { get; set; }
    }

    public class FastBargainResponseDTO
    {
        public string BargainId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // e.g. Pending, Accepted, Rejected, Expired
        public decimal? FinalPrice { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal? OriginalPrice { get; set; }
        public int Quantity { get; set; }
        public string ProductUnitName { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public List<string> ProductImages { get; set; } = new();
        public List<FastBargainProposalDTO> Proposals { get; set; } = new();
    }

    public class FastBargainActionRequestDTO
    {
        public string BargainId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Accept, Reject, Counter
        public decimal? CounterPrice { get; set; }
    }
}
