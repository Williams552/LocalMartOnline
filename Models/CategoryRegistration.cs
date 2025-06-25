using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LocalMartOnline.Models
{
    public enum CategoryRegistrationStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class CategoryRegistration   
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("seller_id")]
        public long SellerId { get; set; }

        [BsonElement("category_name")]
        public string CategoryName { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("image_urls")]
        public List<string> ImageUrls { get; set; } = new();

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public CategoryRegistrationStatus Status { get; set; } = CategoryRegistrationStatus.Pending;

        [BsonElement("rejection_reason")]
        public string? RejectionReason { get; set; }

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}