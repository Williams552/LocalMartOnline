using LocalMartOnline.Models.DTOs;

namespace LocalMartOnline.Models.DTOs.Report
{
    public class ViolatingStoreDto
    {
        public string StoreId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string StoreAddress { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public string MarketAddress { get; set; } = string.Empty;
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int UnderInvestigationReports { get; set; }
        public string SeverityLevel { get; set; } = string.Empty;
        public decimal SeverityScore { get; set; }
        public List<string> ViolationTypes { get; set; } = new();
        public decimal RecentViolationFrequency { get; set; }
        public DateTime? LastViolationDate { get; set; }
        public TimeSpan? AverageResolutionTime { get; set; }
        public string CustomerImpactLevel { get; set; } = string.Empty;
        public List<string> RecommendedActions { get; set; } = new();
        public List<RecentViolationDto> RecentReports { get; set; } = new();
    }

    public class RecentViolationDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ViolationType { get; set; } = string.Empty;
        public DateTime ReportedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }
}