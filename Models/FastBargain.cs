using System;
using System.Collections.Generic;

namespace LocalMartOnline.Models
{
    public enum FastBargainStatus
    {
        Pending,
        Accepted,
        Rejected,
        Expired
    }

    public class FastBargainProposal
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal ProposedPrice { get; set; }
        public DateTime ProposedAt { get; set; }
    }

    public class FastBargain
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string BuyerId { get; set; } = string.Empty;
        public string SellerId { get; set; } = string.Empty;
        public List<FastBargainProposal> Proposals { get; set; } = new List<FastBargainProposal>();
        public FastBargainStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public decimal? FinalPrice { get; set; }
        public int ProposalCount { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
