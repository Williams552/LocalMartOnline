using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
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
        [BsonElement("id")]
        public string Id { get; set; } = string.Empty;
        [BsonElement("user_id")]
        public string UserId { get; set; } = string.Empty;
        [BsonElement("proposed_price")]
        public decimal ProposedPrice { get; set; }
        [BsonElement("proposed_at")]
        public DateTime ProposedAt { get; set; }
    }

    public class FastBargain
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("product_id")]
        public string ProductId { get; set; } = string.Empty;

        [BsonElement("buyer_id")]
        public string BuyerId { get; set; } = string.Empty;

        [BsonElement("seller_id")]
        public string SellerId { get; set; } = string.Empty;

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("proposals")]
        public List<FastBargainProposal> Proposals { get; set; } = new List<FastBargainProposal>();

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public FastBargainStatus Status { get; set; }

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }

        [BsonElement("closed_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ClosedAt { get; set; }

        [BsonElement("final_price")]
        public decimal? FinalPrice { get; set; }

        [BsonElement("proposal_count")]
        public int ProposalCount { get; set; }

        [BsonElement("expires_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ExpiresAt { get; set; }
    }
}
