using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Services;
using LocalMartOnline.Models.DTOs.Report;

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
        /// Get all reports - Admin only
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllReports([FromQuery] GetReportsRequestDto request, [FromHeader] string userRole = "")
        {
            try            {
                // Only Admin can view all reports
                if (userRole != "Admin")
                    return Forbid("Only Admin can view all reports");

                var result = await _reportService.GetAllReportsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving reports", error = ex.Message });
            }
        }

        /// <summary>
        /// Get report by ID - Admin only
        /// </summary>
        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReportById(string reportId, [FromHeader] string userRole = "")
        {
            try
            {                // Only Admin can view report details
                if (userRole != "Admin")
                    return Forbid("Only Admin can view report details");

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
        /// Buyer can report Product or Seller
        /// Seller can report Buyer
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportDto createReportDto, [FromHeader] string userId = "", [FromHeader] string userRole = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { message = "User ID is required" });

                if (string.IsNullOrEmpty(userRole))
                    return BadRequest(new { message = "User role is required" });                // Validate user role
                if (userRole != "Buyer" && userRole != "Seller")
                    return Forbid("Only Buyer and Seller can create reports");

                var report = await _reportService.CreateReportAsync(userId, userRole, createReportDto);
                
                if (report == null)
                    return BadRequest(new { message = "Failed to create report. Invalid target or already reported." });

                return CreatedAtAction(nameof(GetReportById), new { reportId = report.Id }, report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating report", error = ex.Message });
            }        }

        /// <summary>
        /// Get reports statistics - Admin only
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetReportsStatistics([FromHeader] string userRole = "")
        {
            try
            {                // Only Admin can view statistics
                if (userRole != "Admin")
                    return Forbid("Only Admin can view report statistics");

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
        /// Update report status - Admin only
        /// </summary>
        [HttpPut("{reportId}/status")]
        public async Task<IActionResult> UpdateReportStatus(string reportId, [FromBody] UpdateReportStatusDto updateReportStatusDto, [FromHeader] string userRole = "")
        {
            try
            {
                // Only Admin can update report status
                if (userRole != "Admin")
                    return Forbid("Only Admin can update report status");

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
    }
}
