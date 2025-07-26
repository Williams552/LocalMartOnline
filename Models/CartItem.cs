using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalMartOnline.Models
{
    public class CartItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("cart_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CartId { get; set; } = string.Empty;

        [BsonElement("product_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; } = string.Empty;
        [BsonElement("bargain_id")]
        public string? BargainId { get; set; }

        [BsonElement("quantity")]
        public double Quantity { get; set; }

        [BsonElement("bargain_price")]
        public decimal? BargainPrice { get; set; }
        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}