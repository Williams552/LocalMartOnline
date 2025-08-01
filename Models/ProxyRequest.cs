using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalMartOnline.Models
{
    public class ProxyItem
    {
        [BsonElement("name")]
        public string? Name { get; set; }
        [BsonElement("quantity")]
        public decimal Quantity { get; set; }
        [BsonElement("unit")]
        public string? Unit { get; set; }
    }

    public enum ProxyRequestStatus
    {
        Open,        // Buyer vừa tạo – chờ proxy nhận
        Locked,      // Đã có proxy nhận – đang lên đơn
        Completed,      // Đã hoàn tất
        Cancelled    // Buyer hủy yêu cầu
    }

    [BsonIgnoreExtraElements]
    public class ProxyRequest
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("buyer_id"), BsonRepresentation(BsonType.ObjectId)]
        public string BuyerId { get; set; } = string.Empty;

        [BsonElement("items")]
        public List<ProxyItem> Items { get; set; } = new();

        [BsonElement("status")]
        public ProxyRequestStatus Status { get; set; } = ProxyRequestStatus.Open;

        [BsonElement("proxy_shopping_order_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? ProxyShoppingOrderId { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}