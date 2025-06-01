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
    public class ProxyShopperRegistrationController : ControllerBase
    {
        private readonly IRepository<ProxyShopperRegistration> _proxyRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;
        public ProxyShopperRegistrationController(IRepository<ProxyShopperRegistration> proxyRepo, IRepository<User> userRepo, IMapper mapper)
        {
            _proxyRepo = proxyRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] ProxyShopperRegistrationRequestDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var registration = _mapper.Map<ProxyShopperRegistration>(dto);
            registration.UserId = userId!;
            registration.Status = "Pending";
            registration.CreatedAt = DateTime.UtcNow;
            registration.UpdatedAt = DateTime.UtcNow;
            await _proxyRepo.CreateAsync(registration);
            return Ok(new { Message = "Đăng ký proxy shopper thành công. Vui lòng chờ duyệt." });
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyRegistration()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myReg = await _proxyRepo.FindOneAsync(r => r.UserId == userId);
            if (myReg == null) return NotFound();
            var dto = _mapper.Map<ProxyShopperRegistrationRequestDTO>(myReg);
            return Ok(dto);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> GetAll()
        {
            var regs = await _proxyRepo.GetAllAsync();
            var dtos = regs.Select(r => _mapper.Map<ProxyShopperRegistrationRequestDTO>(r));
            return Ok(dtos);
        }

        [HttpPut("approve")]
        [Authorize(Roles = "Admin,MarketStaff")]
        public async Task<IActionResult> Approve([FromBody] ProxyShopperRegistrationApproveDTO dto)
        {
            var reg = await _proxyRepo.FindOneAsync(r => r.Id == dto.RegistrationId);
            if (reg == null) return NotFound();
            reg.Status = dto.Approve ? "Approved" : "Rejected";
            reg.RejectionReason = dto.Approve ? null : dto.RejectionReason;
            reg.UpdatedAt = DateTime.UtcNow;
            await _proxyRepo.UpdateAsync(reg.Id!, reg);
            return Ok(new { Message = "Cập nhật trạng thái đăng ký proxy shopper thành công." });
        }
    }
}
