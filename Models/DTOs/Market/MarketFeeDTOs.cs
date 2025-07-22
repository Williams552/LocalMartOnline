namespace LocalMartOnline.Models.DTOs
{
    public class MarketFeeDto
    {
        public string? Id { get; set; }
        public string MarketId { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public int PaymentDay { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GetMarketFeeRequestDto
    {
        public string? MarketFeeId { get; set; }
        public string? SearchKeyword { get; set; }
    }

    public class MarketFeeCreateDto
    {
        public string MarketId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public int PaymentDay { get; set; }
    }

    public class MarketFeeUpdateDto
    {
        public string? Name { get; set; }
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
        public int? PaymentDay { get; set; }
    }
}
