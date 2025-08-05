namespace LocalMartOnline.Models.DTOs.Store
{
    public class StoreUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
        public string StoreImageUrl { get; set; } = string.Empty;
        public string MarketId { get; set; } = string.Empty;
    }
}