using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
                return Unauthorized("Invalid credentials");
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            var error = await _authService.RegisterAsync(registerDto);
            if (error != null)
                return BadRequest(error);
            return Ok(new { Message = "User registered successfully" });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var result = await _authService.VerifyEmailAsync(token);
            if (!result)
                return BadRequest("Invalid or expired token");
            return Ok(new { Message = "Email verified successfully" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO dto)
        {
            await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(new { Message = "Nếu email tồn tại, hướng dẫn đặt lại mật khẩu đã được gửi." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO dto)
        {
            var result = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!result) return BadRequest("Token không hợp lệ hoặc đã hết hạn.");
            return Ok(new { Message = "Đặt lại mật khẩu thành công." });
        }

        [HttpPost("change-password")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();
            var result = await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
            if (!result) return BadRequest("Mật khẩu hiện tại không đúng hoặc có lỗi.");
            return Ok(new { Message = "Đổi mật khẩu thành công." });
        }
    }
}
