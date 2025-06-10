using System;

namespace LocalMartOnline.Models.DTOs.Product
{
    public class ProductActualPhotoUploadDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsWatermarked { get; set; }
        public DateTime Timestamp { get; set; }
    }
}