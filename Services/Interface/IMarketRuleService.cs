using LocalMartOnline.Models.DTOs.MarketRule;

namespace LocalMartOnline.Services
{
    public interface IMarketRuleService
    {
        // View Market Rules - All roles can view
        Task<GetMarketRulesResponseDto> GetMarketRulesAsync(GetMarketRulesRequestDto request);
        Task<MarketRuleDto?> GetMarketRuleByIdAsync(string ruleId);
        Task<GetMarketRulesResponseDto> GetMarketRulesByMarketIdAsync(string marketId, int page = 1, int pageSize = 10);

        // Add Market Rules - All roles can add
        Task<MarketRuleDto?> CreateMarketRuleAsync(CreateMarketRuleDto createMarketRuleDto);

        // Update Market Rules - All roles can update
        Task<MarketRuleDto?> UpdateMarketRuleAsync(string ruleId, UpdateMarketRuleDto updateMarketRuleDto);

        // Delete Market Rules - All roles can delete
        Task<bool> DeleteMarketRuleAsync(string ruleId);

        // Helper methods
        Task<bool> IsValidMarketIdAsync(string marketId);
        Task<bool> CanUserManageMarketRuleAsync(string userRole);
    }
}
