using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalMartOnline.Models
{
    public class Chat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("sender_id")]
        public string SenderId { get; set; } = string.Empty;

        [BsonElement("receiver_id")]
        public string ReceiverId { get; set; } = string.Empty;
        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
