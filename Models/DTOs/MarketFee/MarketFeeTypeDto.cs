namespace LocalMartOnline.Models.DTOs.MarketFee
{
    public class MarketFeeTypeDto
    {
        public string? Id { get; set; }
        public string FeeType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateMarketFeeTypeDto
    {
        public string FeeType { get; set; } = string.Empty;
    }

    public class UpdateMarketFeeTypeDto
    {
        public string FeeType { get; set; } = string.Empty;
    }

    public class GetMarketFeeTypesResponseDto
    {
        public List<MarketFeeTypeDto> MarketFeeTypes { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
