using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LocalMartOnline.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(IRepository<User> userRepo, IConfiguration configuration, IEmailService emailService)
        {
            _userRepo = userRepo;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<AuthResponseDTO?> LoginAsync(LoginRequestDTO loginDto)
        {
            var user = await _userRepo.FindOneAsync(u => u.Username == loginDto.Username);
            if (user == null || !PasswordHashService.VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;
            if (!user.IsEmailVerified)
                throw new InvalidOperationException("Email chưa xác thực");
            // Cập nhật userToken nếu có gửi lên và khác với DB
            if (!string.IsNullOrEmpty(loginDto.UserToken) && user.UserToken != loginDto.UserToken)
            {
                user.UserToken = loginDto.UserToken;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepo.UpdateAsync(user.Id!, user);
            }
            return new AuthResponseDTO
            {
                Token = GenerateJwtToken(user),
                Role = user.Role,
                Username = user.Username
            };
        }

        public async Task<string?> RegisterAsync(RegisterDTO registerDto)
        {
            var existingByUsername = await _userRepo.FindOneAsync(u => u.Username == registerDto.Username);
            if (existingByUsername != null)
                return "Username already exists";
            var existingByEmail = await _userRepo.FindOneAsync(u => u.Email == registerDto.Email);
            if (existingByEmail != null)
                return "Email already exists";

            var otpToken = Guid.NewGuid().ToString();
            var otpExpiry = DateTime.UtcNow.AddHours(24);
            var newUser = new User
            {
                Username = registerDto.Username,
                PasswordHash = PasswordHashService.HashPassword(registerDto.Password),
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                Role = "Buyer",
                Status = "Active",
                IsEmailVerified = false,
                OTPToken = otpToken,
                OTPExpiry = otpExpiry,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserToken = registerDto.UserToken // Lưu userToken khi đăng ký
            };
            await _userRepo.CreateAsync(newUser);

            // Gửi email xác thực
            var verifyUrl = _configuration["App:BaseUrl"] + "/api/Auth/verify-email?token=" + otpToken;
            var subject = "Xác thực email tài khoản LocalMartOnline";
            var body = $"<p>Chào {registerDto.FullName},</p><p>Vui lòng xác thực email bằng cách nhấn vào link sau: <a href='{verifyUrl}'>Xác thực Email</a></p>";
            await _emailService.SendEmailAsync(registerDto.Email, subject, body);

            return null;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var user = await _userRepo.FindOneAsync(u => u.OTPToken == token && u.OTPExpiry != null && u.OTPExpiry > DateTime.UtcNow);
            if (user == null) return false;
            user.IsEmailVerified = true;
            user.OTPToken = null;
            user.OTPExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user.Id!, user);
            return true;
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userRepo.FindOneAsync(u => u.Email == email);
            if (user == null) return;
            var otpToken = Guid.NewGuid().ToString();
            var otpExpiry = DateTime.UtcNow.AddHours(1); // OTP hết hạn sau 1h
            user.OTPToken = otpToken;
            user.OTPExpiry = otpExpiry;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user.Id!, user);
            var resetUrl = _configuration["App:BaseUrl"] + "/api/Auth/reset-password?token=" + otpToken;
            var subject = "Đặt lại mật khẩu LocalMartOnline";
            var body = $"<p>Bạn vừa yêu cầu đặt lại mật khẩu. Nhấn vào link sau để đặt lại mật khẩu: <a href='{resetUrl}'>Đặt lại mật khẩu</a></p>";
            await _emailService.SendEmailAsync(user.Email, subject, body);
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _userRepo.FindOneAsync(u => u.OTPToken == token && u.OTPExpiry != null && u.OTPExpiry > DateTime.UtcNow);
            if (user == null) return false;
            user.PasswordHash = PasswordHashService.HashPassword(newPassword);
            user.OTPToken = null;
            user.OTPExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user.Id!, user);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userRepo.FindOneAsync(u => u.Id == userId);
            if (user == null) return false;
            if (!PasswordHashService.VerifyPassword(currentPassword, user.PasswordHash)) return false;
            user.PasswordHash = PasswordHashService.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user.Id!, user);
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(12),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
