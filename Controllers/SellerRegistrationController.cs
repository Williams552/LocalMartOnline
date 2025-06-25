using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Repositories;
using System.Security.Claims;
using AutoMapper;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SellerRegistrationController : ControllerBase
    {
        private readonly IRepository<SellerRegistration> _sellerRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;
        public SellerRegistrationController(IRepository<SellerRegistration> sellerRepo, IRepository<User> userRepo, IMapper mapper)
        {
            _sellerRepo = sellerRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] SellerRegistrationRequestDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var registration = _mapper.Map<SellerRegistration>(dto);
            registration.UserId = userId!;
            registration.Status = "Pending";
            registration.CreatedAt = DateTime.UtcNow;
            registration.UpdatedAt = DateTime.UtcNow;
            await _sellerRepo.CreateAsync(registration);
            return Ok(new { success = true, message = "Đăng ký seller thành công. Vui lòng chờ duyệt.", data = (object?)null });
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyRegistration()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myReg = await _sellerRepo.FindOneAsync(r => r.UserId == userId);
            if (myReg == null) return NotFound(new { success = false, message = "Không tìm thấy đăng ký seller của bạn.", data = (object?)null });
            var dto = _mapper.Map<SellerRegistrationRequestDTO>(myReg);
            return Ok(new { success = true, message = "Lấy thông tin đăng ký seller thành công", data = dto });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> GetAll()
        {
            var regs = await _sellerRepo.GetAllAsync();
            var dtos = regs.Select(r => _mapper.Map<SellerRegistrationRequestDTO>(r));
            return Ok(new { success = true, message = "Lấy danh sách đăng ký seller thành công", data = dtos });
        }

        [HttpPut("approve")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> Approve([FromBody] SellerRegistrationApproveDTO dto)
        {
            var reg = await _sellerRepo.FindOneAsync(r => r.Id == dto.RegistrationId);
            if (reg == null) return NotFound(new { success = false, message = "Không tìm thấy đăng ký seller.", data = (object?)null });
            reg.Status = dto.Approve ? "Approved" : "Rejected";
            reg.RejectionReason = dto.Approve ? null : dto.RejectionReason;
            reg.UpdatedAt = DateTime.UtcNow;
            await _sellerRepo.UpdateAsync(reg.Id!, reg);
            return Ok(new { success = true, message = "Cập nhật trạng thái đăng ký seller thành công.", data = (object?)null });
        }

        [HttpGet("profile/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSellerProfile(string userId)
        {
            var reg = await _sellerRepo.FindOneAsync(r => r.UserId == userId && r.Status == "Approved");
            if (reg == null) return NotFound(new { success = false, message = "Không tìm thấy seller profile.", data = (object?)null });
            var user = await _userRepo.FindOneAsync(u => u.Id == userId);
            if (user == null) return NotFound(new { success = false, message = "Không tìm thấy user.", data = (object?)null });
            var sellerProfile = new SellerProfileDTO
            {
                StoreName = reg.StoreName,
                StoreAddress = reg.StoreAddress,
                MarketId = reg.MarketId,
                BusinessLicense = reg.BusinessLicense,
                Username = user.Username,
                AvatarUrl = user.AvatarUrl
            };
            return Ok(new { success = true, message = "Lấy seller profile thành công", data = sellerProfile });
        }
    }
}
