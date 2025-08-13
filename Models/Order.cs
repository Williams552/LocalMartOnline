using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace LocalMartOnline.Models
{
    public enum OrderStatus
    {
        Pending,    // Người mua mới đặt hàng
        Confirmed,  // Người bán xác nhận còn hàng
        Paid,       // Người bán xác nhận đã nhận được tiền
        Completed,  // Người mua xác nhận đã nhận đúng hàng
        Cancelled   // Đơn hàng bị hủy
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }


    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }


        [BsonElement("buyer_id")]
        public string BuyerId { get; set; } = string.Empty; // ObjectId as string

        [BsonElement("seller_id")]
        public string SellerId { get; set; } = string.Empty; // ObjectId as string

        [BsonElement("total_amount")]
        public decimal TotalAmount { get; set; }

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        [BsonElement("payment_status")]
        [BsonRepresentation(BsonType.String)]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("cancel_reason")]
        public string? CancelReason { get; set; } = string.Empty;

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updated_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}