namespace LocalMartOnline.Models.DTOs.Report
{    public class ReportDto
    {
        public string Id { get; set; } = string.Empty;
        public string ReporterId { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateReportDto
    {
        public string TargetType { get; set; } = string.Empty; // Product, Seller, Buyer
        public string TargetId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;    }

    public class UpdateReportStatusDto
    {
        public string Status { get; set; } = string.Empty; // Pending, Resolved, Dismissed
    }

    public class GetReportsResponseDto
    {
        public List<ReportDto> Reports { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class GetReportsRequestDto
    {
        public string? ReporterId { get; set; }
        public string? TargetType { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
