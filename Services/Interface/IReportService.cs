using System.Threading.Tasks;
using System.Collections.Generic;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.Report;

namespace LocalMartOnline.Services.Interface
{
    public interface IReportService
    {
        Task<object> GetRevenueStatisticsAsync(string from, string to);
        Task<ReportFileDto> ExportRevenueReportAsync(string from, string to);
        Task<string> GenerateReportAsync(GenerateReportRequestDto dto);
        Task<ReportFileDto> ExportReportAsync(string reportId);
        Task<object> GetMarketSalesReportAsync(string marketId, string from, string to);
        Task<int> GetNumberOfSellersAsync(string marketId);
        Task<IEnumerable<ViolatingStoreDto>> GetViolatingStoresAsync(string marketId);
        Task<GetReportsResponseDto> GetAllReportsAsync(GetReportsRequestDto request);
        Task<ReportDto?> GetReportByIdAsync(string reportId);
        Task<ReportDto?> CreateReportAsync(string reporterId, string reporterRole, CreateReportDto createReportDto);
        Task<ReportDto?> UpdateReportStatusAsync(string reportId, UpdateReportStatusDto updateReportStatusDto);
        Task<GetReportsResponseDto> GetReportsByReporterAsync(string reporterId, int page = 1, int pageSize = 10);
    }
}
