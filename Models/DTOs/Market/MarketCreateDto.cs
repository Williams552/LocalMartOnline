namespace LocalMartOnline.Models.DTOs.Market;
using System;

public class MarketCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? OperatingHours { get; set; }
    public string? ContactInfo { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}