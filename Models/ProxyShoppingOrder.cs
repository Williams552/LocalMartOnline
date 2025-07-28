using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Models
{
    public enum Status
    {
        Pending,    // Người mua mới đặt hàng
        Confirmed,  // Người bán xác nhận còn hàng
        Paid,       // Người bán xác nhận đã nhận được tiền
        Completed,  // Người mua xác nhận đã nhận đúng hàng
        Cancelled   // Đơn hàng bị hủy
    }
    [BsonIgnoreExtraElements]
    public class ProxyShoppingOrder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("buyer_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BuyerId { get; set; } = string.Empty;

        [BsonElement("proxy_shopper_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ProxyShopperId { get; set; }

        [BsonElement("delivery_address")]
        public string DeliveryAddress { get; set; } = string.Empty;
        [BsonElement("items")]
        public List<ProductDto> Items { get; set; } = new();
        [BsonElement("total_amount")]
        public decimal? TotalAmount { get; set; }

        [BsonElement("status")]
        public Status Status { get; set; } = Status.Pending; // 'Pending', 'Confirmed', 'Completed', 'Cancelled'

        [BsonElement("proxy_fee")]
        public decimal? ProxyFee { get; set; }

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
