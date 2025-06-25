namespace LocalMartOnline.Models.DTOs.License
{
    public class SellerLicenseDto
    {
        public string Id { get; set; } = string.Empty; // license_id
        public string RegistrationId { get; set; } = string.Empty; // registration_id
        public string UserId { get; set; } = string.Empty; // From SellerRegistrations
        public string SellerName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public string? LicenseNumber { get; set; }
        public string LicenseUrl { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? IssuingAuthority { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsExpired => ExpiryDate.HasValue && DateTime.UtcNow > ExpiryDate.Value;
        public int? DaysUntilExpiry => ExpiryDate?.Subtract(DateTime.UtcNow).Days;
    }

    public class CreateSellerLicenseDto
    {
        public string RegistrationId { get; set; } = string.Empty; // SellerRegistrations Id
        public string LicenseType { get; set; } = string.Empty; // BusinessLicense, FoodSafetyCertificate, TaxRegistration, EnvironmentalPermit, Other
        public string? LicenseNumber { get; set; }
        public string LicenseUrl { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? IssuingAuthority { get; set; }
    }

    public class UpdateSellerLicenseDto
    {
        public string LicenseType { get; set; } = string.Empty;
        public string? LicenseNumber { get; set; }
        public string LicenseUrl { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? IssuingAuthority { get; set; }
    }

    public class ReviewSellerLicenseDto
    {
        public string Status { get; set; } = string.Empty; // Verified, Rejected
        public string? RejectionReason { get; set; }
    }

    public class GetSellerLicensesRequestDto
    {
        public string? RegistrationId { get; set; }
        public string? UserId { get; set; }
        public string? LicenseType { get; set; }
        public string? Status { get; set; }
        public bool? IsExpired { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetSellerLicensesResponseDto
    {
        public List<SellerLicenseDto> Licenses { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class SellerLicenseStatisticsDto
    {
        public int TotalLicenses { get; set; }
        public int PendingLicenses { get; set; }
        public int VerifiedLicenses { get; set; }
        public int RejectedLicenses { get; set; }
        public int ExpiredLicenses { get; set; }
        public int ExpiringLicenses { get; set; } // Expiring within 30 days
        public Dictionary<string, int> LicensesByType { get; set; } = new();
        public Dictionary<string, int> LicensesByStatus { get; set; } = new();
    }
}
