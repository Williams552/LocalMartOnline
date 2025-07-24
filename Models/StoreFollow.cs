using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LocalMartOnline.Models
{
    public class StoreFollow
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user_id")]
        public string UserId { get; set; } = string.Empty; // ✅ Chuyển từ long thành string

        [BsonElement("store_id")]
        public string StoreId { get; set; } = string.Empty; // ✅ Chuyển từ long thành string

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}