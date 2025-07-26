using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Models.DTOs.Product;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IProductService
    {
        Task<ProductDto> AddProductAsync(ProductCreateDto dto);
        Task<bool> EditProductAsync(string id, ProductUpdateDto dto);
        Task<bool> ToggleProductAsync(string id, bool enable);
        Task<bool> DeleteProductAsync(string id);
        Task<PagedResultDto<ProductDto>> GetAllProductsAsync(int page, int pageSize);
        Task<ProductDto?> GetProductDetailsAsync(string id);
        Task<bool> UploadActualProductPhotoAsync(ProductActualPhotoUploadDto dto);
        Task<PagedResultDto<ProductDto>> SearchProductsAsync(string keyword, string? categoryId, decimal? latitude, decimal? longitude, int page, int pageSize);
        Task<PagedResultDto<ProductDto>> FilterProductsAsync(ProductFilterDto filter);
        Task<PagedResultDto<ProductDto>> GetProductsByStoreAsync(string storeId, int page, int pageSize);
        Task<PagedResultDto<ProductDto>> GetProductsByMarketAsync(string marketId, int page, int pageSize);
        Task<PagedResultDto<ProductDto>> FilterProductsInStoreAsync(ProductFilterDto filter);
        Task<PagedResultDto<ProductDto>> SearchProductsInStoreAsync(string storeId, string keyword, int page, int pageSize);
        //Task<ProductDto?> GetProductDetailsInStoreAsync(string storeId, string productId);
        Task<SearchProductResultDto> SearchProductsAsync(SearchProductRequestDto request);
        Task<PagedResultDto<ProductDto>> FilterProductsInMarketAsync(string marketId, ProductFilterDto filter);
        Task<PagedResultDto<ProductDto>> SearchProductsInMarketAsync(string marketId, string keyword, int page, int pageSize);
        
        // Seller methods - Include all products (active and inactive)
        Task<PagedResultDto<ProductDto>> GetAllProductsForSellerAsync(string storeId, int page, int pageSize);
        Task<PagedResultDto<ProductDto>> SearchProductsForSellerAsync(string storeId, string keyword, int page, int pageSize);
        Task<PagedResultDto<ProductDto>> FilterProductsForSellerAsync(ProductFilterDto filter);
    }
}
