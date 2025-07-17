namespace LocalMartOnline.Models.DTOs
{
    public class MarketFeeDto
    {
        public string? Id { get; set; }
        public string MarketId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class MarketFeeCreateDto
    {
        public string MarketId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class MarketFeeUpdateDto
    {
        public string? Name { get; set; }
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
    }
}
