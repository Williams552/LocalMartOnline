using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LocalMartOnline.Models
{
    [BsonIgnoreExtraElements]
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("buyer_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BuyerId { get; set; } = string.Empty;

        [BsonElement("total_amount")]
        public decimal TotalAmount { get; set; }

        [BsonElement("delivery_address")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; // 'Pending', 'Preparing', 'Delivering', 'Completed', 'Cancelled'

        [BsonElement("payment_status")]
        public string PaymentStatus { get; set; } = "Pending"; // 'Pending', 'Completed', 'Failed'

        [BsonElement("expected_delivery_time")]
        public DateTime? ExpectedDeliveryTime { get; set; }

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
