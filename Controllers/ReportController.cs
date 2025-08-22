using LocalMartOnline.Models.DTOs.Report;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        /// <summary>
        /// Get total number of API endpoints in the project
        /// </summary>
        [HttpGet("endpoint-count")]
        [AllowAnonymous]
        public IActionResult GetEndpointCount([FromServices] Microsoft.AspNetCore.Mvc.ApiExplorer.IApiDescriptionGroupCollectionProvider provider)
        {
            var count = provider.ApiDescriptionGroups.Items.SelectMany(g => g.Items).Count();
            return Ok(new { endpointCount = count });
        }

        /// <summary>
        /// Get all reports
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllReports([FromQuery] GetReportsRequestDto request)
        {
            try
            {
                var result = await _reportService.GetAllReportsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving reports", error = ex.Message });
            }
        }

        /// <summary>
        /// Get report by ID
        /// </summary>
        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReportById(string reportId)
        {
            try
            {
                var report = await _reportService.GetReportByIdAsync(reportId);

                if (report == null)
                    return NotFound(new { message = "Report not found" });

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving report", error = ex.Message });
            }
        }

        /// <summary>
        /// Get reports by reporter (user can see their own reports)
        /// </summary>
        [HttpGet("my-reports")]
        public async Task<IActionResult> GetMyReports([FromHeader] string userId = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { message = "User ID is required" });

                var result = await _reportService.GetReportsByReporterAsync(userId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving your reports", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new report
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportDto createReportDto, [FromHeader] string userId = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { message = "User ID is required" });

                var report = await _reportService.CreateReportAsync(userId, createReportDto);

                if (report == null)
                    return BadRequest(new { message = "Failed to create report. Invalid target or already reported." });

                return CreatedAtAction(nameof(GetReportById), new { reportId = report.Id }, report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating report", error = ex.Message });
            }
        }

        /// <summary>
        /// Get reports statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetReportsStatistics()
        {
            try
            {
                var pendingReports = await _reportService.GetAllReportsAsync(new GetReportsRequestDto { Status = "Pending", PageSize = 1000 });
                var resolvedReports = await _reportService.GetAllReportsAsync(new GetReportsRequestDto { Status = "Resolved", PageSize = 1000 });
                var dismissedReports = await _reportService.GetAllReportsAsync(new GetReportsRequestDto { Status = "Dismissed", PageSize = 1000 });

                var statistics = new
                {
                    TotalReports = pendingReports.TotalCount + resolvedReports.TotalCount + dismissedReports.TotalCount,
                    PendingReports = pendingReports.TotalCount,
                    ResolvedReports = resolvedReports.TotalCount,
                    DismissedReports = dismissedReports.TotalCount,
                    ReportsByType = new
                    {
                        ProductReports = await GetReportCountByType("Product"),
                        StoreReports = await GetReportCountByType("Store"),
                        SellerReports = await GetReportCountByType("Seller"),
                        BuyerReports = await GetReportCountByType("Buyer")
                    }
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving report statistics", error = ex.Message });
            }
        }

        private async Task<int> GetReportCountByType(string targetType)
        {
            var reports = await _reportService.GetAllReportsAsync(new GetReportsRequestDto { TargetType = targetType, PageSize = 1000 });
            return reports.TotalCount;
        }

        /// <summary>
        /// Update report status
        /// </summary>
        [HttpPut("{reportId}/status")]
        public async Task<IActionResult> UpdateReportStatus(string reportId, [FromBody] UpdateReportStatusDto updateReportStatusDto)
        {
            try
            {
                if (updateReportStatusDto == null)
                    return BadRequest(new { message = "Update data is required" });

                // Validate status values
                var validStatuses = new[] { "Pending", "Resolved", "Dismissed" };
                if (!validStatuses.Contains(updateReportStatusDto.Status))
                    return BadRequest(new { message = "Invalid status. Valid values are: Pending, Resolved, Dismissed" });

                var updatedReport = await _reportService.UpdateReportStatusAsync(reportId, updateReportStatusDto);

                if (updatedReport == null)
                    return NotFound(new { message = "Report not found or failed to update" });

                return Ok(updatedReport);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating report status", error = ex.Message });
            }
        }

        /// <summary>
        /// Debug database collections (temporary endpoint for troubleshooting)
        /// </summary>
        [HttpGet("debug/database")]
        public async Task<IActionResult> DebugDatabase()
        {
            try
            {
                // Cast to concrete type to access debug method
                if (_reportService is LocalMartOnline.Services.Implement.ReportService reportServiceImpl)
                {
                    var debugInfo = await reportServiceImpl.DebugDatabaseAsync();
                    return Ok(debugInfo);
                }
                else
                {
                    return BadRequest(new { message = "Debug method not available for this service implementation" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in debug database", error = ex.Message });
            }
        }

        [HttpGet("seller-count")]
        public async Task<IActionResult> GetSellerMetrics([FromQuery] string? marketId = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            try
            {
                var metrics = await _reportService.GetNumberOfSellersAsync(marketId, from, to);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving seller metrics", error = ex.Message });
            }
        }
        /// <summary>
        /// Get market sales report
        /// </summary>
        [HttpGet("market/{marketId}/sales")]
        [Authorize(Roles = "Admin,MarketManagementBoardHead,LocalGovernmentRepresentative")]
        public async Task<IActionResult> GetMarketSalesReport(
            string marketId,
            [FromQuery] string from,
            [FromQuery] string to)
        {
            try
            {
                if (!MongoDB.Bson.ObjectId.TryParse(marketId, out _))
                {
                    return BadRequest(new { message = "Invalid market ID" });
                }

                var report = await _reportService.GetMarketSalesReportAsync(marketId, from, to);
                return Ok(new
                {
                    success = true,
                    message = "Lấy báo cáo doanh số chợ thành công",
                    data = report
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving market sales report",
                    error = ex.Message
                });
            }
        }

        [HttpGet("violating-stores")]
        [Authorize(Roles = "Admin,MarketManagementBoardHead")]
        public async Task<IActionResult> GetViolatingStores(
    [FromQuery] string? marketId = null)
        {
            var violatingStores = await _reportService.GetViolatingStoresAsync(marketId);
            return Ok(new { success = true, message = "Lấy danh sách cửa hàng vi phạm thành công", data = violatingStores });
        }

        [HttpGet("product-statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProductStatistics(
    [FromQuery] string? categoryId = null,
    [FromQuery] string period = "30d")
        {
            try
            {
                var statistics = await _reportService.GetProductStatisticsAsync(categoryId, period);
                return Ok(new
                {
                    success = true,
                    message = "Lấy thống kê sản phẩm thành công",
                    data = statistics
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy thống kê sản phẩm",
                    error = ex.Message
                });
            }
        }

        [HttpGet("category-statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCategoryStatistics([FromQuery] string period = "30d")
        {
            try
            {
                var statistics = await _reportService.GetCategoryStatisticsAsync(period);
                return Ok(new
                {
                    success = true,
                    message = "Lấy thống kê danh mục thành công",
                    data = statistics
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy thống kê danh mục",
                    error = ex.Message
                });
            }
        }

        [HttpGet("order-statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderStatistics(
    [FromQuery] string period = "30d",
    [FromQuery] string? status = null)
        {
            try
            {
                var statistics = await _reportService.GetOrderStatisticsAsync(period, status);
                return Ok(new
                {
                    success = true,
                    message = "Lấy thống kê đơn hàng thành công",
                    data = statistics
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy thống kê đơn hàng",
                    error = ex.Message
                });
            }
        }
    }
}