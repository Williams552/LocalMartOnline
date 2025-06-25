using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalMartOnline.Models
{
    [BsonIgnoreExtraElements]
    public class Store
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("seller_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string SellerId { get; set; } = string.Empty;

        [BsonElement("market_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MarketId { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("latitude")]
        public decimal? Latitude { get; set; }

        [BsonElement("longitude")]
        public decimal? Longitude { get; set; }

        [BsonElement("contact_number")]
        public string? ContactNumber { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Open"; // 'Open', 'Closed', 'Suspended'

        [BsonElement("rating")]
        public decimal Rating { get; set; } = 0.0m;

        [BsonElement("store_image_url")]
        public string? StoreImageUrl { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
