using LocalMartOnline.Models.DTOs.Common;

namespace LocalMartOnline.Models.DTOs.Product
{
    public class ProductFilterDto : PagedRequestDto
    {
        public string? StoreId { get; set; }
        public string? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Keyword { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Status { get; set; } // "Active", "OutOfStock", "Inactive"
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public double MaxDistanceKm { get; set; }
        public string? SortBy { get; set; } // "price", "name", "status", "created", "updated"
        public bool? Ascending { get; set; } = true;
        public new int Page { get; set; } = 1;
        public new int PageSize { get; set; } = 20;
    }
}