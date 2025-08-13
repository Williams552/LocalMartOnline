using System.ComponentModel.DataAnnotations;

namespace LocalMartOnline.Models.DTOs
{
    public class SellerRegistrationRequestDTO
    {
        [Required]
        public string StoreName { get; set; } = string.Empty;
        [Required]
        public string StoreAddress { get; set; } = string.Empty;
        [Required]
        public string MarketId { get; set; } = string.Empty;
        [Required]

        public string? BusinessLicense { get; set; }
    }

    public class SellerRegistrationApproveDTO
    {
        [Required]
        public string RegistrationId { get; set; } = string.Empty;
        [Required]
        public bool Approve { get; set; }
        public string? RejectionReason { get; set; }

        // Chỉ required khi Approve = true
        public DateTime? LicenseEffectiveDate { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
    }

    public class SellerRegistrationResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string MarketName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string StoreAddress { get; set; } = string.Empty;
        public string MarketId { get; set; } = string.Empty;
        public string? BusinessLicense { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public DateTime? LicenseEffectiveDate { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }

        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}