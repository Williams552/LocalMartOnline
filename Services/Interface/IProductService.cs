using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Services.Interface
{
    public interface IProductService
    {
        // ...existing code...
        Task<SearchProductResultDto> SearchProductsAsync(SearchProductRequestDto request);
    }
}