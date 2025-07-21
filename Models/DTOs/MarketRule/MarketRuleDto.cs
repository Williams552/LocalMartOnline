namespace LocalMartOnline.Models.DTOs.MarketRule
{
    public class MarketRuleDto
    {
        public string Id { get; set; } = string.Empty;
        public string MarketId { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateMarketRuleDto
    {
        public string MarketId { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateMarketRuleDto
    {
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class GetMarketRulesResponseDto
    {
        public List<MarketRuleDto> MarketRules { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class GetMarketRulesRequestDto
    {
        public string? MarketId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchKeyword { get; set; }
    }
}
