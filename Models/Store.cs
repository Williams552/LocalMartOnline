using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LocalMartOnline.Models
{
    public class Store
    {
        [BsonId]
        [BsonElement("store_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("seller_id")]
        public string SellerId { get; set; }

        [BsonElement("market_id")]
        public string MarketId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("latitude")]
        public decimal Latitude { get; set; }

        [BsonElement("longitude")]
        public decimal Longitude { get; set; }

        [BsonElement("contact_number")]
        public string ContactNumber { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = "Open";

        [BsonElement("rating")]
        public decimal Rating { get; set; } = 0.0m;

        [BsonElement("store_image_url")]
        public string StoreImageUrl { get; set; } = string.Empty;

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}