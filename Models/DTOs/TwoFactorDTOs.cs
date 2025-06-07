using System.ComponentModel.DataAnnotations;

namespace LocalMartOnline.Models.DTOs
{
    public class TwoFactorRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class TwoFactorVerifyDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(10, MinimumLength = 4)]
        public string OtpCode { get; set; } = string.Empty;
    }
}
