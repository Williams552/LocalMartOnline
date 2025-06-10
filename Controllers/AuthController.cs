using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid payload", data = (object?)null });
            }
            try
            {
                var loginResult = await _authService.LoginAsync(loginDto);
                if (loginResult == null)
                    return Unauthorized(new { success = false, message = "Invalid credentials", data = (object?)null });

                var user = await _authService.GetUserByUsernameOrEmailAsync(loginDto.Username);
                if (user != null && user.TwoFactorEnabled)
                {
                    if (string.IsNullOrEmpty(user.Email))
                    {
                        return BadRequest(new { success = false, message = "Tài khoản không có email để gửi mã xác thực 2 bước.", data = (object?)null });
                    }
                    await _authService.Send2FACodeAsync(user.Email);
                    return Ok(new { success = true, message = "Vui lòng kiểm tra email để nhập mã xác thực 2 bước.", requires2FA = true, data = (object?)null });
                }

                return Ok(new { success = true, message = "Đăng nhập thành công", data = loginResult });
            }
            catch (InvalidOperationException ex) when (ex.Message == "Email chưa xác thực")
            {
                return BadRequest(new { success = false, message = "Email chưa xác thực. Vui lòng kiểm tra email để xác thực tài khoản.", data = (object?)null });
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            var error = await _authService.RegisterAsync(registerDto);
            if (error != null)
                return BadRequest(new { success = false, message = error, data = (object?)null });
            return Ok(new { success = true, message = "User registered successfully", data = (object?)null });
        }

        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var result = await _authService.VerifyEmailAsync(token);
            if (!result)
                return BadRequest(new { success = false, message = "Invalid or expired token", data = (object?)null });
            return Ok(new { success = true, message = "Email verified successfully", data = (object?)null });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO dto)
        {
            await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(new { success = true, message = "Nếu email tồn tại, hướng dẫn đặt lại mật khẩu đã được gửi.", data = (object?)null });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO dto)
        {
            var result = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!result) return BadRequest(new { success = false, message = "Token không hợp lệ hoặc đã hết hạn.", data = (object?)null });
            return Ok(new { success = true, message = "Đặt lại mật khẩu thành công.", data = (object?)null });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized(new { success = false, message = "Unauthorized", data = (object?)null });
            var result = await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
            if (!result) return BadRequest(new { success = false, message = "Mật khẩu hiện tại không đúng hoặc có lỗi.", data = (object?)null });
            return Ok(new { success = true, message = "Đổi mật khẩu thành công.", data = (object?)null });
        }

        [HttpPost("2fa/verify")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify2FA([FromBody] TwoFactorVerifyDTO dto)
        {
            var isValid = await _authService.Verify2FACodeAsync(dto.Email, dto.OtpCode);
            if (!isValid)
                return BadRequest(new { success = false, message = "Mã xác thực không đúng hoặc đã hết hạn.", data = (object?)null });

            var user = await _authService.GetUserByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { success = false, message = "Tài khoản không tồn tại.", data = (object?)null });
            var token = _authService.GenerateJwtTokenFor2FA(user);
            return Ok(new { success = true, message = "Xác thực 2 bước thành công.", data = new AuthResponseDTO { Token = token, Role = user.Role, Username = user.Username } });
        }
    }
}
