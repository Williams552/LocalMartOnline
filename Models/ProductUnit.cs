using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LocalMartOnline.Models
{
    public class ProductUnit
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty; // "kg", "con", "chai"

        [BsonElement("display_name")]
        public string DisplayName { get; set; } = string.Empty; // "Kilogram", "Con", "Chai"

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("requires_integer_quantity")]
        public bool RequiresIntegerQuantity { get; set; } = false;

        [BsonElement("unit_type")]
        [BsonRepresentation(BsonType.String)]
        public UnitType UnitType { get; set; } = UnitType.Count;

        [BsonElement("is_active")]
        public bool IsActive { get; set; } = true;

        [BsonElement("sort_order")]
        public int SortOrder { get; set; } = 0; // Để sắp xếp hiển thị

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum UnitType
    {
        Weight,   // Khối lượng (kg, gram)
        Volume,   // Thể tích (lít, ml)
        Count,    // Số lượng (con, chai, gói)
        Length    // Chiều dài (mét, cm)
    }
}