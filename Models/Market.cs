using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalMartOnline.Models
{
    [BsonIgnoreExtraElements]
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
        public string Status { get; set; } = "Active"; // Active, Suspended

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
