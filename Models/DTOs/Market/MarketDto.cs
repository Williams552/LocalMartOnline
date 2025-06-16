namespace LocalMartOnline.Models.DTOs.Market;
public class MarketDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? OperatingHours { get; set; }
    public string? ContactInfo { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string Status { get; set; } = "Active";
    public int StallCount { get; set; }
}