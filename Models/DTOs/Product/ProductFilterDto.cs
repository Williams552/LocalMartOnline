using LocalMartOnline.Models.DTOs.Common;

namespace LocalMartOnline.Models.DTOs.Product
{
    public class ProductFilterDto : PagedRequestDto
    {
        public string? StoreId { get; set; }
        public string? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public double? MaxDistanceKm { get; set; }
        public string? Name { get; set; }
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
        public string? Status { get; set; }
        public string? Keyword { get; set; }
        public string? SortBy { get; set; }
        public bool? Ascending { get; set; }
    }
}