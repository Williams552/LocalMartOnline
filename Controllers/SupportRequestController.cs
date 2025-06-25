using LocalMartOnline.Models.DTOs.SupportRequest;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupportRequestController : ControllerBase
    {
        private readonly ISupportRequestService _supportRequestService;

        public SupportRequestController(ISupportRequestService supportRequestService)
        {
            _supportRequestService = supportRequestService;
        }

        /// <summary>
        /// Get all support requests (for Market Staff)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupportRequestDto>>> GetAllSupportRequests()
        {
            try
            {
                var supportRequests = await _supportRequestService.GetAllSupportRequestsAsync();
                return Ok(new
                {
                    success = true,
                    message = "Support requests retrieved successfully",
                    data = supportRequests
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving support requests",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get support requests by status (for Market Staff)
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<SupportRequestDto>>> GetSupportRequestsByStatus(string status)
        {
            try
            {
                var validStatuses = new[] { "Open", "InProgress", "Resolved" };
                if (!validStatuses.Contains(status))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid status. Valid statuses are: Open, InProgress, Resolved"
                    });
                }

                var supportRequests = await _supportRequestService.GetSupportRequestsByStatusAsync(status);
                return Ok(new
                {
                    success = true,
                    message = $"Support requests with status '{status}' retrieved successfully",
                    data = supportRequests
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving support requests",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get support request by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SupportRequestDto>> GetSupportRequestById(string id)
        {
            try
            {
                var supportRequest = await _supportRequestService.GetSupportRequestByIdAsync(id);
                if (supportRequest == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Support request not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Support request retrieved successfully",
                    data = supportRequest
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the support request",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get support requests by user ID (for users to view their own requests)
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<SupportRequestDto>>> GetSupportRequestsByUserId(string userId)
        {
            try
            {
                var supportRequests = await _supportRequestService.GetSupportRequestsByUserIdAsync(userId);
                return Ok(new
                {
                    success = true,
                    message = "User support requests retrieved successfully",
                    data = supportRequests
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving user support requests",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Create a new support request (for users)
        /// </summary>
        [HttpPost("user/{userId}")]
        public async Task<ActionResult> CreateSupportRequest(string userId, [FromBody] CreateSupportRequestDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid input data",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var supportRequestId = await _supportRequestService.CreateSupportRequestAsync(userId, createDto);
                
                return CreatedAtAction(
                    nameof(GetSupportRequestById),
                    new { id = supportRequestId },
                    new
                    {
                        success = true,
                        message = "Support request created successfully",
                        data = new { id = supportRequestId }
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the support request",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Respond to a support request (for Market Staff)
        /// </summary>
        [HttpPut("{id}/respond")]
        public async Task<ActionResult> RespondToSupportRequest(string id, [FromBody] RespondToSupportRequestDto responseDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid input data",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var validStatuses = new[] { "InProgress", "Resolved" };
                if (!validStatuses.Contains(responseDto.Status))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid status. Valid statuses for response are: InProgress, Resolved"
                    });
                }

                var result = await _supportRequestService.RespondToSupportRequestAsync(id, responseDto);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Support request not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Response added successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while responding to the support request",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update support request status (for Market Staff)
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateSupportRequestStatus(string id, [FromBody] UpdateSupportRequestStatusDto statusDto)
        {
            try
            {
                var validStatuses = new[] { "Open", "InProgress", "Resolved" };
                if (!validStatuses.Contains(statusDto.Status))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid status. Valid statuses are: Open, InProgress, Resolved"
                    });
                }

                var result = await _supportRequestService.UpdateSupportRequestStatusAsync(id, statusDto.Status);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Support request not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Support request status updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the support request status",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete a support request (for Admin)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSupportRequest(string id)
        {
            try
            {
                var result = await _supportRequestService.DeleteSupportRequestAsync(id);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Support request not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Support request deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the support request",
                    error = ex.Message
                });
            }
        }
    }
}
