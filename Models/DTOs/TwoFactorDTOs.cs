namespace LocalMartOnline.Models.DTOs
{
    public class TwoFactorRequestDTO
    {
        public string Email { get; set; }
    }

    public class TwoFactorVerifyDTO
    {
        public string Email { get; set; }
        public string OtpCode { get; set; }
    }
}
