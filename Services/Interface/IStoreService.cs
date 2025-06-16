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
        Task<bool> SuspendStoreAsync(string id, string reason);
        Task<bool> ReactivateStoreAsync(string id);
        Task<StoreDto> CreateStoreAsync(StoreCreateDto dto);
        Task<bool> CloseStoreAsync(string id);
        Task<bool> UpdateStoreAsync(string id, StoreUpdateDto dto);
        Task<bool> FollowStoreAsync(long userId, long storeId);
        Task<bool> UnfollowStoreAsync(long userId, long storeId);
        Task<IEnumerable<StoreDto>> GetFollowingStoresAsync(long userId);
        Task<StoreDto?> GetStoreProfileAsync(string id);
    }
}