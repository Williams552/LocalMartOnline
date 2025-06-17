namespace LocalMartOnline.Models.DTOs.Favorite
{
    public class AddToFavoriteRequestDto
    {
        public string ProductId { get; set; } = string.Empty;
    }

    public class FavoriteProductDto
    {
        public string FavoriteId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string StoreId { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime AddedToFavoriteAt { get; set; }
    }

    public class GetFavoriteProductsResponseDto
    {
        public List<FavoriteProductDto> FavoriteProducts { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class FavoriteActionResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
