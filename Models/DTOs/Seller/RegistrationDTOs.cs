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
        public string? BusinessLicense { get; set; }
    }

    public class SellerRegistrationApproveDTO
    {
        [Required]
        public string RegistrationId { get; set; } = string.Empty;
        [Required]
        public bool Approve { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class ProxyShopperRegistrationRequestDTO
    {
        [Required]
        public string OperatingArea { get; set; } = string.Empty;
        [Required]
        public string TransportMethod { get; set; } = string.Empty;
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class ProxyShopperRegistrationApproveDTO
    {
        [Required]
        public string RegistrationId { get; set; } = string.Empty;
        [Required]
        public bool Approve { get; set; }
        public string? RejectionReason { get; set; }
    }
}
