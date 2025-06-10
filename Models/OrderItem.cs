using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LocalMartOnline.Models
{
    public class OrderItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("order_id")]
        public string OrderId { get; set; } = string.Empty;

        [BsonElement("product_id")]
        public string ProductId { get; set; } = string.Empty;

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("price_at_purchase")]
        public decimal PriceAtPurchase { get; set; }
    }
}