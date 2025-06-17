using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalMartOnline.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("store_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? StoreId { get; set; }

        [BsonElement("category_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? CategoryId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("stock_quantity")]
        public int StockQuantity { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Active";

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}