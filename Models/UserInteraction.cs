using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalMartOnline.Models
{
    public enum InteractionValue
    {
        Click = 1,
        Like = 2,
        AddToCart = 3,
        Purchase = 4
    }

    public class UserInteraction
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid InteractionId { get; set; }


        [BsonElement("UserId")]
        public required string UserId { get; set; }

        [BsonElement("ProductId")]
        public required string ProductId { get; set; }

        [BsonElement("Type")]
        public required string Type { get; set; } // vclick/add_cart/purchase/search/...

        [BsonRepresentation(BsonType.Int32)]
        [BsonElement("Value")]
        public InteractionValue Value { get; set; }
    }
}
