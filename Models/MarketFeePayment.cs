using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LocalMartOnline.Models
{
    public enum MarketFeePaymentStatus
    {
        Pending,
        Completed,
        Failed
    }

    [BsonIgnoreExtraElements]
    public class MarketFeePayment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PaymentId { get; set; } = string.Empty;
        
        [BsonElement("seller_id")]
        public string SellerId { get; set; } = string.Empty;
        
        [BsonElement("fee_id")]
        public string FeeId { get; set; } = string.Empty;
        
        [BsonElement("amount")]
        public decimal Amount { get; set; }
        
        [BsonElement("payment_status")]
        [BsonRepresentation(BsonType.String)]
        public MarketFeePaymentStatus PaymentStatus { get; set; } = MarketFeePaymentStatus.Pending;
        
        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [BsonElement("due_date")]
        public DateTime DueDate { get; set; } = DateTime.Now.AddMonths(1);
        
        [BsonElement("payment_date")]
        public DateTime? PaymentDate { get; set; }
        
        [BsonElement("admin_notes")]
        public string? AdminNotes { get; set; }
    }
}