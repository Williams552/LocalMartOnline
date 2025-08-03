using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LocalMartOnline.Models
{
    public class ProxyShopperRegistration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("store_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? StoreId { get; set; }

        [BsonElement("operating_area")]
        public string OperatingArea { get; set; } = string.Empty;

        [BsonElement("transport_method")]
        public string TransportMethod { get; set; } = string.Empty;

        [BsonElement("payment_method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [BsonElement("rejection_reason")]
        public string? RejectionReason { get; set; }

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updated_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
