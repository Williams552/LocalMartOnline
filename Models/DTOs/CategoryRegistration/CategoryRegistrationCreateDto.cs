namespace LocalMartOnline.Models.DTOs.CategoryRegistration
{
    public class CategoryRegistrationCreateDto
    {
        public long SellerId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new(); // URLs or base64 strings
    }
}