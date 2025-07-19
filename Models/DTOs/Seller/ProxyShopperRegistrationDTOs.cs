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