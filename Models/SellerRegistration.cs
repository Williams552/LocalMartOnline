using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LocalMartOnline.Models
{
    public class SellerRegistration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("store_name")]
        public string StoreName { get; set; } = string.Empty;

        [BsonElement("store_address")]
        public string StoreAddress { get; set; } = string.Empty;

        [BsonElement("market_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MarketId { get; set; } = string.Empty;

        [BsonElement("business_license")]
        public string? BusinessLicense { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [BsonElement("rejection_reason")]
        public string? RejectionReason { get; set; }

        [BsonElement("license_effective_date")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? LicenseEffectiveDate { get; set; }

        [BsonElement("license_expiry_date")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? LicenseExpiryDate { get; set; }

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
