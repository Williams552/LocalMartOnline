using LocalMartOnline.Models.DTOs.Report;

namespace LocalMartOnline.Services.Interface
{
    public interface IReportService
    {
        Task<GetReportsResponseDto> GetAllReportsAsync(GetReportsRequestDto request);
        Task<ReportDto?> GetReportByIdAsync(string reportId);      
        Task<ReportDto?> CreateReportAsync(string reporterId, string reporterRole, CreateReportDto createReportDto);
        Task<ReportDto?> UpdateReportStatusAsync(string reportId, UpdateReportStatusDto updateReportStatusDto);
        Task<GetReportsResponseDto> GetReportsByReporterAsync(string reporterId, int page = 1, int pageSize = 10);
    }
}
