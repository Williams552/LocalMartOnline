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
    }
}

