<<<<<<< HEAD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalMartOnline.Models
{
=======
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace LocalMartOnline.Models
{
    public enum ProductStatus
    {
        Active,
        Inactive
    }

>>>>>>> origin/develop
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("store_id")]
<<<<<<< HEAD
        [BsonRepresentation(BsonType.ObjectId)]
        public string? StoreId { get; set; }

        [BsonElement("category_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? CategoryId { get; set; }
=======
        public string StoreId { get; set; } = string.Empty; // ObjectId as string

        [BsonElement("category_id")]
        public string CategoryId { get; set; } = string.Empty; // ObjectId as string
>>>>>>> origin/develop

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
<<<<<<< HEAD
        public string? Description { get; set; }
=======
        public string Description { get; set; } = string.Empty;
>>>>>>> origin/develop

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("stock_quantity")]
        public int StockQuantity { get; set; }

        [BsonElement("status")]
<<<<<<< HEAD
        public string Status { get; set; } = "Active";

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
=======
        [BsonRepresentation(BsonType.String)]
        public ProductStatus Status { get; set; } = ProductStatus.Active;

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
>>>>>>> origin/develop
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}