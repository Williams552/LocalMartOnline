namespace LocalMartOnline.Models.DTOs.Store
{
    public class StoreSearchFilterDto
    {
        // Tìm kiếm cơ bản
        public string? Keyword { get; set; }
        public string? CategoryId { get; set; }
        
        // Lọc theo đánh giá
        public decimal? MinRating { get; set; }
        public decimal? MaxRating { get; set; }
        
        // Lọc theo vị trí
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? MaxDistance { get; set; }  // Khoảng cách tính theo km
        
        // Lọc theo trạng thái (chỉ admin mới sử dụng được)
        public string? Status { get; set; }
        
        // Lọc theo chợ
        public string? MarketId { get; set; }
        
        // Phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        
        // Sắp xếp
        public string? SortBy { get; set; } // "rating", "distance", "created"
        public bool Ascending { get; set; } = false;
    }
}