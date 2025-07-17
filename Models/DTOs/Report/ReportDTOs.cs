namespace LocalMartOnline.Models.DTOs
{
    public class ReportFileDto
    {
        public byte[] Content { get; set; } = new byte[0];
        public string ContentType { get; set; } = "application/octet-stream";
        public string FileName { get; set; } = string.Empty;
    }

    public class GenerateReportRequestDto
    {
        public string ReportType { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string? MarketId { get; set; }
    }

    public class ViolatingStoreDto
    {
        public string StoreId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
