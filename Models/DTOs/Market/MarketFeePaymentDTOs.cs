namespace LocalMartOnline.Models.DTOs
{
    public class MarketFeePaymentDto
    {
        public long PaymentId { get; set; }
        public long SellerId { get; set; }
        public string FeeId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class MarketFeePaymentCreateDto
    {
        public long SellerId { get; set; }
        public string FeeId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
