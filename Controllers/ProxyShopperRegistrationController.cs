using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Repositories;
using System.Security.Claims;
using AutoMapper;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Models.DTOs.Seller;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProxyShopperRegistrationController : ControllerBase
    {
        private readonly IProxyShopperService _proxyShopperService;
        private readonly IMapper _mapper;
        public ProxyShopperRegistrationController(IProxyShopperService proxyShopperService, IMapper mapper)
        {
            _proxyShopperService = proxyShopperService;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] ProxyShopperRegistrationRequestDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _proxyShopperService.RegisterProxyShopperAsync(dto, userId!);
            return Ok(new { success = true, message = "Đăng ký proxy shopper thành công. Vui lòng chờ duyệt.", data = (object?)null });
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyRegistration()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dto = await _proxyShopperService.GetMyRegistrationAsync(userId!);
            if (dto == null)
                return NotFound(new { success = false, message = "Không tìm thấy đăng ký proxy shopper của bạn.", data = (object?)null });
            return Ok(new { success = true, message = "Lấy thông tin đăng ký proxy shopper thành công", data = dto });
        }

        [HttpGet]
        [Authorize(Roles = "Admin, MS, LGR, MMBH")]
        public async Task<IActionResult> GetAll()
        {
            var dtos = await _proxyShopperService.GetAllRegistrationsAsync();
            return Ok(new { success = true, message = "Lấy danh sách đăng ký proxy shopper thành công", data = dtos });
        }

        [HttpPut("approve")]
        [Authorize(Roles = "Admin, MS, LGR, MMBH")]
        public async Task<IActionResult> Approve([FromBody] ProxyShopperRegistrationApproveDTO dto)
        {
            var success = await _proxyShopperService.ApproveRegistrationAsync(dto);
            if (!success)
                return NotFound(new { success = false, message = "Không tìm thấy đăng ký proxy shopper.", data = (object?)null });
            return Ok(new { success = true, message = "Cập nhật trạng thái đăng ký proxy shopper thành công.", data = (object?)null });
        }
    }
}
