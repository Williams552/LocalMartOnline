using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalMartOnline.Models
{
    [BsonIgnoreExtraElements]
    public class OrderItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("order_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string OrderId { get; set; } = string.Empty;

        [BsonElement("product_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; } = string.Empty;

        [BsonElement("quantity")]
        public decimal Quantity { get; set; }

        [BsonElement("price_at_purchase")]
        public decimal PriceAtPurchase { get; set; }
    }
}
