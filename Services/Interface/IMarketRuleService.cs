using LocalMartOnline.Models.DTOs.MarketRule;

namespace LocalMartOnline.Services.Interface
{
    public interface IMarketRuleService
    {
        // View Market Rules - All roles can view
        Task<GetMarketRulesResponseDto> GetMarketRulesAsync(GetMarketRulesRequestDto request);
        Task<MarketRuleDto?> GetMarketRuleByIdAsync(string ruleId);
        Task<GetMarketRulesResponseDto> GetMarketRulesByMarketIdAsync(string marketId, int page = 1, int pageSize = 10);

        // Add Market Rules - All roles can add
        Task<MarketRuleDto?> CreateMarketRuleAsync(string userId, CreateMarketRuleDto createMarketRuleDto);

        // Update Market Rules - All roles can update
        Task<MarketRuleDto?> UpdateMarketRuleAsync(string userId, string ruleId, UpdateMarketRuleDto updateMarketRuleDto);

        // Delete Market Rules - All roles can delete
        Task<bool> DeleteMarketRuleAsync(string userId, string ruleId);

        // Helper methods
        Task<bool> IsValidMarketIdAsync(string marketId);
        Task<bool> CanUserManageMarketRuleAsync(string userId, string userRole);
    }
}
