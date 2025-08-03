namespace LocalMartOnline.Models.DTOs.Payment
{
    public class PaymentCallbackDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Success, Failed
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Notes { get; set; }
    }

    public class PendingPaymentDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string FeeTypeName { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string PaymentStatus { get; set; } = "Pending"; // Trạng thái từ MarketFeePayment
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class CreatePaymentUrlRequestDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
    }

    public class CreatePaymentUrlResponseDto
    {
        public string PaymentUrl { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
    }
}
