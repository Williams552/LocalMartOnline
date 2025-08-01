using LocalMartOnline.Models.DTOs.MarketFee;

namespace LocalMartOnline.Services.Interface
{
    public interface IMarketFeeTypeService
    {
        Task<GetMarketFeeTypesResponseDto> GetAllMarketFeeTypesAsync();
        Task<MarketFeeTypeDto?> GetMarketFeeTypeByIdAsync(string id);
        Task<MarketFeeTypeDto> CreateMarketFeeTypeAsync(CreateMarketFeeTypeDto createDto);
        Task<MarketFeeTypeDto?> UpdateMarketFeeTypeAsync(string id, UpdateMarketFeeTypeDto updateDto);
        Task<bool> DeleteMarketFeeTypeAsync(string id);
        Task<bool> RestoreMarketFeeTypeAsync(string id);
    }
}
