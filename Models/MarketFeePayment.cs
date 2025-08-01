using System;

namespace LocalMartOnline.Models
{
    public enum MarketFeePaymentStatus
    {
        Pending,
        Completed,
        Failed
    }

    public class MarketFeePayment
    {
        public long PaymentId { get; set; }
        public long SellerId { get; set; }
        public string FeeId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public MarketFeePaymentStatus PaymentStatus { get; set; } = MarketFeePaymentStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}