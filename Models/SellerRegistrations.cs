using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalMartOnline.Models
{
    [BsonIgnoreExtraElements]
    public class SellerRegistrations
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user_id")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("store_name")]
        public string StoreName { get; set; } = string.Empty;

        [BsonElement("store_address")]
        public string StoreAddress { get; set; } = string.Empty;

        [BsonElement("market_id")]
        public string MarketId { get; set; } = string.Empty; // Reference to Markets Id

        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [BsonElement("rejection_reason")]
        public string? RejectionReason { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
