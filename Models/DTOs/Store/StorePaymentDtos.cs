using System;
using System.Collections.Generic;

namespace LocalMartOnline.Models.DTOs.Store
{
    public class StoreWithPaymentInfoDto
    {
        public string PaymentId { get; set; } = string.Empty; // Payment ID từ MarketFeePayment
        public string StoreName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string SellerPhone { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public string FeeTypeName { get; set; } = string.Empty; // Tên loại phí từ MarketFeeType
        public decimal MonthlyRentalFee { get; set; } // Phí thuê tháng từ MarketFee
        public DateTime DueDate { get; set; } // Hạn chót thanh toán
        public string PaymentStatus { get; set; } = "Pending"; // Trạng thái thanh toán
        public DateTime? PaymentDate { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class GetAllStoresWithPaymentRequestDto
    {
        public string? MarketId { get; set; }
        public string? FeeTypeId { get; set; } // Filter by fee type
        public string? PaymentStatus { get; set; } // Pending, Completed, Failed
        public int? Month { get; set; } // Filter by month (1-12)
        public int? Year { get; set; } // Filter by year
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchKeyword { get; set; } // Search by store name, seller name
    }

    public class GetAllStoresWithPaymentResponseDto
    {
        public List<StoreWithPaymentInfoDto> Stores { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    public class UpdateStorePaymentStatusDto
    {
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
    }
}
