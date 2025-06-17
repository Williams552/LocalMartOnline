using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LocalMartOnline.Models
{
    [BsonIgnoreExtraElements]
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("target_type")]
        public string TargetType { get; set; } = string.Empty; // 'Product', 'Seller', 'ProxyShopper'

        [BsonElement("target_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TargetId { get; set; } = string.Empty;

        [BsonElement("rating")]
        public int Rating { get; set; } // 1 to 5

        [BsonElement("comment")]
        public string? Comment { get; set; }

        [BsonElement("response")]
        public string? Response { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum ReviewTargetType
    {
        Product,
        Seller,
        ProxyShopper
    }
}
