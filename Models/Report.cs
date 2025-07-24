using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalMartOnline.Models
{
    [BsonIgnoreExtraElements]
    public class Report
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("reporter_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ReporterId { get; set; } = string.Empty;

        [BsonElement("target_type")]
        public string TargetType { get; set; } = string.Empty; // Product, Seller, Buyer

        [BsonElement("target_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TargetId { get; set; } = string.Empty;

        [BsonElement("reason")]
        public string Reason { get; set; } = string.Empty;
        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; // Pending, Resolved, Dismissed

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
