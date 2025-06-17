using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LocalMartOnline.Models
{
    [BsonIgnoreExtraElements]
    public class ProxyShoppingOrder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("buyer_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BuyerId { get; set; } = string.Empty;

        [BsonElement("proxy_shopper_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ProxyShopperId { get; set; }

        [BsonElement("delivery_address")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [BsonElement("total_amount")]
        public decimal? TotalAmount { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; // 'Pending', 'Accepted', 'Completed', 'Cancelled'

        [BsonElement("proxy_fee")]
        public decimal? ProxyFee { get; set; }

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
