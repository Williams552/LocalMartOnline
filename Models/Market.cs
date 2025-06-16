using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LocalMartOnline.Models
{
    public class Market
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("operating_hours")]
        public string? OperatingHours { get; set; }

        [BsonElement("contact_info")]
        public string? ContactInfo { get; set; }

        [BsonElement("latitude")]
        public decimal? Latitude { get; set; }

        [BsonElement("longitude")]
        public decimal? Longitude { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Active"; // "Active" or "Suspended"

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}