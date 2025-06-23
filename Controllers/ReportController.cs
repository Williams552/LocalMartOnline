using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // 1. View Revenue Statistics
        [HttpGet("revenue-statistics")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> GetRevenueStatistics([FromQuery] string from, [FromQuery] string to)
        {
            var result = await _reportService.GetRevenueStatisticsAsync(from, to);
            return Ok(result);
        }

        // 2. Export Revenue Report
        [HttpGet("export-revenue")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> ExportRevenueReport([FromQuery] string from, [FromQuery] string to)
        {
            var file = await _reportService.ExportRevenueReportAsync(from, to);
            return File(file.Content, file.ContentType, file.FileName);
        }

        // 3. Generate Report
        [HttpPost("generate")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> GenerateReport([FromBody] GenerateReportRequestDto dto)
        {
            var reportId = await _reportService.GenerateReportAsync(dto);
            return Ok(new { success = true, reportId });
        }

        // 4. Export Report
        [HttpGet("export")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> ExportReport([FromQuery] string reportId)
        {
            var file = await _reportService.ExportReportAsync(reportId);
            return File(file.Content, file.ContentType, file.FileName);
        }

        // 5. View Market Sales Report
        [HttpGet("market-sales")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> GetMarketSalesReport([FromQuery] string marketId, [FromQuery] string from, [FromQuery] string to)
        {
            var result = await _reportService.GetMarketSalesReportAsync(marketId, from, to);
            return Ok(result);
        }

        // 6. View Number of Sellers
        [HttpGet("number-of-sellers")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> GetNumberOfSellers([FromQuery] string marketId)
        {
            var result = await _reportService.GetNumberOfSellersAsync(marketId);
            return Ok(result);
        }

        // 7. View List of Violating Stores
        [HttpGet("violating-stores")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> GetViolatingStores([FromQuery] string marketId)
        {
            var result = await _reportService.GetViolatingStoresAsync(marketId);
            return Ok(result);
        }
    }
}
