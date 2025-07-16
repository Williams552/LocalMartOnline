using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Models.DTOs.Store;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IStoreService
    {
        Task<PagedResultDto<StoreDto>> GetAllStoresAsync(int page, int pageSize);
        Task<PagedResultDto<StoreDto>> GetSuspendedStoresAsync(int page, int pageSize);
        Task<StoreDto?> GetStoreBySellerAsync(string sellerId);
        Task<bool> SuspendStoreAsync(string id, string reason);
        Task<bool> ReactivateStoreAsync(string id);
        Task<StoreDto> CreateStoreAsync(StoreCreateDto dto);
        Task<bool> CloseStoreAsync(string id);
        Task<bool> UpdateStoreAsync(string id, StoreUpdateDto dto);
        // Thay đổi các method signatures từ long thành string
        Task<bool> FollowStoreAsync(string userId, string storeId); // ✅ long → string
        Task<bool> UnfollowStoreAsync(string userId, string storeId); // ✅ long → string
        Task<IEnumerable<StoreDto>> GetFollowingStoresAsync(string userId); // ✅ long → string
        Task<bool> IsFollowingStoreAsync(string userId, string storeId); // ✅ long → string
        Task<PagedResultDto<object>> GetStoreFollowersAsync(string storeId, int page, int pageSize); // ✅ long → string
        Task<StoreDto?> GetStoreProfileAsync(string id);
        Task<PagedResultDto<StoreDto>> GetActiveStoresByMarketIdAsync(string marketId, int page, int pageSize);
        Task<bool> ToggleStoreStatusAsync(string id);
        
        // Thêm phương thức tìm kiếm và lọc
        Task<PagedResultDto<StoreDto>> SearchStoresAsync(StoreSearchFilterDto searchFilter, bool isAdmin = false);
        
        // Tìm kiếm cửa hàng theo khoảng cách
        Task<PagedResultDto<StoreDto>> FindStoresNearbyAsync(decimal latitude, decimal longitude, 
            decimal maxDistanceKm, int page = 1, int pageSize = 20);
        
        Task<PagedResultDto<StoreDto>> GetActiveStoresAsync(int page, int pageSize);
        Task<bool> HasExistingStoreAsync(string sellerId);
        
        // Get store statistics and featured products
        Task<object> GetStoreStatisticsAsync(string storeId);
    }
}