using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Services;
using LocalMartOnline.Models.DTOs.License;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SellerLicenseController : ControllerBase
    {
        private readonly ISellerLicenseService _sellerLicenseService;

        public SellerLicenseController(ISellerLicenseService sellerLicenseService)
        {
            _sellerLicenseService = sellerLicenseService;
        }

        /// <summary>
        /// Create a new seller license - Seller, Admin, MarketManagementBoardHead
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSellerLicense([FromBody] CreateSellerLicenseDto createLicenseDto, [FromHeader] string userId = "", [FromHeader] string userRole = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    return BadRequest(new { message = "User ID and role are required" });

                var validCreatorRoles = new[] { "Seller", "Admin", "MarketManagementBoardHead" };
                if (!validCreatorRoles.Contains(userRole))
                    return Forbid("Only Seller, Admin, and MarketManagementBoardHead can create licenses");

                if (createLicenseDto == null)
                    return BadRequest(new { message = "License data is required" });

                var license = await _sellerLicenseService.CreateSellerLicenseAsync(userId, userRole, createLicenseDto);
                
                if (license == null)
                    return BadRequest(new { message = "Failed to create license. Invalid registration or license type already exists." });

                return CreatedAtAction(nameof(GetSellerLicenseById), new { licenseId = license.Id }, license);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating license", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all seller licenses - Admin, MarketManagementBoardHead
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllSellerLicenses([FromQuery] GetSellerLicensesRequestDto request, [FromHeader] string userRole = "")
        {
            try
            {
                var validViewerRoles = new[] { "Admin", "MarketManagementBoardHead" };
                if (!validViewerRoles.Contains(userRole))
                    return Forbid("Only Admin and MarketManagementBoardHead can view all licenses");

                var result = await _sellerLicenseService.GetAllSellerLicensesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving licenses", error = ex.Message });
            }
        }

        /// <summary>
        /// Get seller license by ID - Admin, MarketManagementBoardHead, Seller (own licenses)
        /// </summary>
        [HttpGet("{licenseId}")]
        public async Task<IActionResult> GetSellerLicenseById(string licenseId, [FromHeader] string userId = "", [FromHeader] string userRole = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    return BadRequest(new { message = "User ID and role are required" });

                var license = await _sellerLicenseService.GetSellerLicenseByIdAsync(licenseId);
                
                if (license == null)
                    return NotFound(new { message = "License not found" });

                // Check permission
                var canView = await _sellerLicenseService.CanUserManageSellerLicenseAsync(userId, userRole, license.RegistrationId);
                if (!canView)
                    return Forbid("You don't have permission to view this license");

                return Ok(license);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving license", error = ex.Message });
            }
        }        /// <summary>
        /// Get licenses by registration ID - Admin, MarketManagementBoardHead, Seller (own registration)
        /// </summary>
        [HttpGet("registration/{registrationId}")]
        public async Task<IActionResult> GetLicensesByRegistration(string registrationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromHeader] string userId = "", [FromHeader] string userRole = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    return BadRequest(new { message = "User ID and role are required" });

                // Check permission
                var canView = await _sellerLicenseService.CanUserManageSellerLicenseAsync(userId, userRole, registrationId);
                if (!canView)
                    return Forbid("You don't have permission to view these licenses");

                var result = await _sellerLicenseService.GetSellerLicensesByRegistrationAsync(registrationId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving licenses", error = ex.Message });
            }
        }

        /// <summary>
        /// Get my licenses - Seller only
        /// </summary>
        [HttpGet("my-licenses")]
        public async Task<IActionResult> GetMyLicenses([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromHeader] string userId = "", [FromHeader] string userRole = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { message = "User ID is required" });

                if (userRole != "Seller")
                    return Forbid("Only Seller can access this endpoint");

                var result = await _sellerLicenseService.GetSellerLicensesByUserAsync(userId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving your licenses", error = ex.Message });
            }
        }

        /// <summary>
        /// Update seller license - Only for Pending status
        /// </summary>
        [HttpPut("{licenseId}")]
        public async Task<IActionResult> UpdateSellerLicense(string licenseId, [FromBody] UpdateSellerLicenseDto updateLicenseDto, [FromHeader] string userId = "", [FromHeader] string userRole = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    return BadRequest(new { message = "User ID and role are required" });

                if (updateLicenseDto == null)
                    return BadRequest(new { message = "Update data is required" });

                var license = await _sellerLicenseService.UpdateSellerLicenseAsync(licenseId, userId, userRole, updateLicenseDto);
                
                if (license == null)
                    return BadRequest(new { message = "Failed to update license. License not found, not pending, or insufficient permissions." });

                return Ok(license);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating license", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete seller license - Seller, Admin, MarketManagementBoardHead
        /// </summary>
        [HttpDelete("{licenseId}")]
        public async Task<IActionResult> DeleteSellerLicense(string licenseId, [FromHeader] string userId = "", [FromHeader] string userRole = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    return BadRequest(new { message = "User ID and role are required" });

                var success = await _sellerLicenseService.DeleteSellerLicenseAsync(licenseId, userId, userRole);
                
                if (!success)
                    return BadRequest(new { message = "Failed to delete license. License not found or insufficient permissions." });

                return Ok(new { message = "License deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting license", error = ex.Message });
            }
        }

        /// <summary>
        /// Review seller license (Verify/Reject) - Admin, MarketManagementBoardHead, MarketStaff
        /// </summary>
        [HttpPut("{licenseId}/review")]
        public async Task<IActionResult> ReviewSellerLicense(string licenseId, [FromBody] ReviewSellerLicenseDto reviewDto, [FromHeader] string userId = "", [FromHeader] string userRole = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    return BadRequest(new { message = "User ID and role are required" });

                var validReviewerRoles = new[] { "Admin", "MarketManagementBoardHead", "MarketStaff" };
                if (!validReviewerRoles.Contains(userRole))
                    return Forbid("Only Admin, MarketManagementBoardHead, and MarketStaff can review licenses");

                if (reviewDto == null)
                    return BadRequest(new { message = "Review data is required" });

                // Validate status
                var validStatuses = new[] { "Verified", "Rejected" };
                if (!validStatuses.Contains(reviewDto.Status))
                    return BadRequest(new { message = "Invalid status. Valid values: Verified, Rejected" });

                // If rejecting, reason is required
                if (reviewDto.Status == "Rejected" && string.IsNullOrEmpty(reviewDto.RejectionReason))
                    return BadRequest(new { message = "Rejection reason is required when rejecting a license" });

                var license = await _sellerLicenseService.ReviewSellerLicenseAsync(licenseId, userId, userRole, reviewDto);
                
                if (license == null)
                    return BadRequest(new { message = "Failed to review license. License not found or not pending." });

                return Ok(license);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error reviewing license", error = ex.Message });
            }
        }

        /// <summary>
        /// Get seller license statistics - Admin, MarketManagementBoardHead
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetSellerLicenseStatistics([FromHeader] string userRole = "")
        {
            try
            {
                var validViewerRoles = new[] { "Admin", "MarketManagementBoardHead" };
                if (!validViewerRoles.Contains(userRole))
                    return Forbid("Only Admin and MarketManagementBoardHead can view license statistics");

                var statistics = await _sellerLicenseService.GetSellerLicenseStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving license statistics", error = ex.Message });
            }
        }
    }
}
