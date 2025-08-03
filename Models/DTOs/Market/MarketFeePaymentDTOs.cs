namespace LocalMartOnline.Models.DTOs
{
    public class MarketFeePaymentDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string SellerId { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public string FeeId { get; set; } = string.Empty;
        public string FeeName { get; set; } = string.Empty;
        public string FeeTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class MarketFeePaymentCreateDto
    {
        public string SellerId { get; set; } = string.Empty;
        public string FeeId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class SellerPaymentStatusDto
    {
        public string SellerId { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public decimal MonthlyRentalFee { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public DateTime? LastPaymentDate { get; set; }
        public string FeeTypeName { get; set; } = "Phí thuê tháng";
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class GetSellersPaymentStatusRequestDto
    {
        public string MarketId { get; set; } = string.Empty;
        public string? PaymentStatus { get; set; } // Pending, Completed, Failed
        public int Month { get; set; } = DateTime.Now.Month;
        public int Year { get; set; } = DateTime.Now.Year;
    }

    public class GetSellersPaymentStatusResponseDto
    {
        public List<SellerPaymentStatusDto> Sellers { get; set; } = new();
        public int TotalSellers { get; set; }
        public decimal TotalAmountDue { get; set; }
        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
        public int OverdueCount { get; set; }
    }

    public class UpdatePaymentStatusDto
    {
        public string SellerId { get; set; } = string.Empty;
        public string MarketId { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty; // Pending, Completed, Failed
        public int Month { get; set; } = DateTime.Now.Month;
        public int Year { get; set; } = DateTime.Now.Year;
    }

    public class GetAllMarketFeePaymentsRequestDto
    {
        public string? MarketId { get; set; }
        public string? FeeTypeId { get; set; }
        public string? PaymentStatus { get; set; } // Pending, Completed, Failed
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchKeyword { get; set; } // Search by seller name, store name, market name
    }

    public class GetAllMarketFeePaymentsResponseDto
    {
        public List<MarketFeePaymentDetailDto> Payments { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    public class MarketFeePaymentDetailDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public string MarketId { get; set; } = string.Empty;
        public string FeeTypeName { get; set; } = string.Empty;
        public string FeeTypeId { get; set; } = string.Empty;
        public string FeeName { get; set; } = string.Empty;
        public decimal FeeAmount { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreId { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string SellerId { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public DateTime? PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class UpdatePaymentStatusByIdDto
    {
        public string PaymentStatus { get; set; } = string.Empty; // Pending, Completed, Failed
        public DateTime? PaymentDate { get; set; }
    }

    public class AdminCreatePaymentDto
    {
        public string UserId { get; set; } = string.Empty; // User ID (không nhất thiết phải là seller)
        public string FeeId { get; set; } = string.Empty; // Market Fee ID
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string? Notes { get; set; } // Ghi chú từ Admin
    }

    public class AdminCreatePaymentResponseDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FeeName { get; set; } = string.Empty;
        public string FeeTypeName { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
