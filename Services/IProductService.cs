using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Services
{
    public interface IProductService
    {
        // ...existing code...
        Task<SearchProductResultDto> SearchProductsAsync(SearchProductRequestDto request);
    }
}