using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Models
{
    public enum ProxyOrderStatus
    {
        Draft,      // Proxy đang soạn đơn
        Proposed,   // Đã gửi đề xuất cho Buyer
        Paid,       // Buyer thanh toán
        InProgress, // Proxy đi mua
        Completed,  // Đã giao và xác nhận
        Cancelled,  // Bị hủy
        Expired     // Quá hạn xử lý
    }
    [BsonIgnoreExtraElements]
    public class ProxyShoppingOrder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonElement("proxy_request_id"), BsonRepresentation(BsonType.ObjectId)]
        public string ProxyRequestId { get; set; } = string.Empty;
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
        public ProxyOrderStatus Status { get; set; } = ProxyOrderStatus.Draft;
        [BsonElement("proxy_fee")]
        public decimal? ProxyFee { get; set; }

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("proof_images")]
        public string? ProofImages { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
