using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LocalMartOnline.Models.DTOs.Seller
{
    public class ProxyShopperRegistrationRequestDTO
    {
        [Required]
        public string MarketId { get; set; } = string.Empty;
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

    public class ProxyShopperRegistrationResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public string TransportMethod { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
    }
}