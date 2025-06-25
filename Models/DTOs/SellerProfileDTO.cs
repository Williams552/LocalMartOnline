namespace LocalMartOnline.Models.DTOs
{
    public class SellerProfileDTO
    {
        public string StoreName { get; set; } = string.Empty;
        public string StoreAddress { get; set; } = string.Empty;
        public string MarketId { get; set; } = string.Empty;
        public string? BusinessLicense { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        // Có thể bổ sung các trường public khác nếu cần
    }
}
