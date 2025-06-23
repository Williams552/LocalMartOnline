using System.Threading.Tasks;
using System.Collections.Generic;
using LocalMartOnline.Models.DTOs;

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
    }
}
