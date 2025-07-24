using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IMarketFeeService
    {
        Task<IEnumerable<MarketFeeDto>> GetAllAsync(GetMarketFeeRequestDto request);
        Task<MarketFeeDto?> GetByIdAsync(string id);
        Task<MarketFeeDto> CreateAsync(MarketFeeCreateDto dto);
        Task<bool> UpdateAsync(string id, MarketFeeUpdateDto dto);
        Task<bool> DeleteAsync(string id);
        Task<bool> PayFeeAsync(MarketFeePaymentDto dto);
    }
}
