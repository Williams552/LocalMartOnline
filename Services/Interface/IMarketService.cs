using LocalMartOnline.Models.DTOs.Market;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IMarketService
    {
        Task<MarketDto> CreateAsync(MarketCreateDto dto);
        Task<IEnumerable<MarketDto>> GetAllAsync();
        Task<MarketDto?> GetByIdAsync(string id);
        Task<bool> UpdateAsync(string id, MarketUpdateDto dto);
        Task<bool> DeleteAsync(string id);
        Task<IEnumerable<MarketDto>> SearchAsync(string keyword);
        Task<IEnumerable<MarketDto>> FilterAsync(string? status, string? area, int? minStalls, int? maxStalls);
        Task<bool> ToggleStatusAsync(string id);
        Task<IEnumerable<MarketDto>> GetActiveMarketsAsync();
        
        // Phương thức mới cho người dùng
        Task<IEnumerable<MarketDto>> SearchActiveMarketsAsync(string keyword);
        Task<IEnumerable<MarketDto>> FilterActiveMarketsAsync(string? area, int? minStalls, int? maxStalls);
    }
}

