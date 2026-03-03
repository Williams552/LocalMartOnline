using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Repositories;
using System.Security.Claims;
using AutoMapper;
using LocalMartOnline.Services.Interface;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SellerRegistrationController : ControllerBase
    {
        private readonly ISellerRegistrationervice _service;
        public SellerRegistrationController(ISellerRegistrationervice service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] SellerRegistrationRequestDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _service.RegisterAsync(userId!, dto);
            return Ok(new { success = true, message = "Đăng ký seller thành công. Vui lòng chờ duyệt.", data = (object?)null });
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyRegistration()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dto = await _service.GetMyRegistrationAsync(userId!);
            if (dto == null) return NotFound(new { success = false, message = "Không tìm thấy đăng ký seller của bạn.", data = (object?)null });
            return Ok(new { success = true, message = "Lấy thông tin đăng ký seller thành công", data = dto });
        }

        [HttpGet]
        [Authorize(Roles = "Admin, MS, LGR, MMBH")]
        public async Task<IActionResult> GetAll()
        {
            var dtos = await _service.GetAllRegistrationsAsync();
            return Ok(new { success = true, message = "Lấy danh sách đăng ký seller thành công", data = dtos });
        }

        [HttpPut("approve")]
        [Authorize(Roles = "Admin, MS, LGR, MMBH")]
        public async Task<IActionResult> Approve([FromBody] SellerRegistrationApproveDTO dto)
        {
            try
            {
                // Validation khi approve = true
                if (dto.Approve)
                {
                    if (!dto.LicenseEffectiveDate.HasValue || !dto.LicenseExpiryDate.HasValue)
                    {
                        return BadRequest(new 
                        { 
                            success = false, 
                            message = "Ngày hiệu lực và ngày hết hạn giấy phép là bắt buộc khi phê duyệt.",
                            data = (object?)null 
                        });
                    }
                }

                var result = await _service.ApproveAsync(dto);
                if (!result)
                    return NotFound(new { success = false, message = "Không tìm thấy đăng ký seller.", data = (object?)null });
                
                var message = dto.Approve ? "Phê duyệt đăng ký seller thành công." : "Từ chối đăng ký seller thành công.";
                return Ok(new { success = true, message = message, data = (object?)null });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message, data = (object?)null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi xử lý đăng ký.", error = ex.Message });
            }
        }

        [HttpGet("profile/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSellerProfile(string userId)
        {
            var sellerProfile = await _service.GetSellerProfileAsync(userId);
            if (sellerProfile == null)
                return NotFound(new { success = false, message = "Không tìm thấy seller profile.", data = (object?)null });
            return Ok(new { success = true, message = "Lấy seller profile thành công", data = sellerProfile });
        }
    }
}
