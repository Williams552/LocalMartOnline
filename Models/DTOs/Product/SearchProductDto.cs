using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocalMartOnline.Models.DTOs.Product
{
    public class SearchProductRequestDto
    {
        public string? Search { get; set; } = string.Empty;
        public string? CategoryId { get; set; }
        public string SortPrice { get; set; } = string.Empty; // "asc", "desc"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class SearchProductResponseDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string StoreId { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    // Sử dụng SearchProductResultDto duy nhất ở đây, các nơi khác dùng lại class này
    public class SearchProductResultDto
    {
        public List<SearchProductResponseDto> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}