using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace LocalMartOnline.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IRepository<User> userRepo, IConfiguration configuration, IEmailService emailService, IDistributedCache cache, ILogger<AuthService> logger)
        {
            _userRepo = userRepo;
            _configuration = configuration;
            _emailService = emailService;
            _cache = cache;
            _logger = logger;
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
                // Validate UserToken before updating
                if (!string.IsNullOrEmpty(loginDto.UserToken) && user.UserToken != loginDto.UserToken)
                {
                    // Example: UserToken must be 10-200 chars, alphanumeric (customize as needed)
                    var token = loginDto.UserToken;
                    bool isValidToken = token.Length >= 10 && token.Length <= 200 && token.All(char.IsLetterOrDigit);
                    if (isValidToken)
                    {
                        user.UserToken = token;
                        user.UpdatedAt = DateTime.UtcNow;
                        await _userRepo.UpdateAsync(user.Id!, user);
                    }
                    // else: ignore invalid token, do not update
                }
            }
            return new AuthResponseDTO
            {
                Token = GenerateJwtToken(user),
                Role = user.Role,
                Username = user.Username
            };
        }

        public async Task<string?> RegisterAsync(RegisterDTO registerDto, string baseUrl)
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
            var verifyUrl = baseUrl + "/api/Auth/verify-email?token=" + otpToken;
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

        public async Task ForgotPasswordAsync(string email, string baseUrl)
        {
            var user = await _userRepo.FindOneAsync(u => u.Email == email);
            if (user == null) return;
            var otpToken = Guid.NewGuid().ToString();
            var otpExpiry = DateTime.UtcNow.AddHours(1); // OTP hết hạn sau 1h
            user.OTPToken = otpToken;
            user.OTPExpiry = otpExpiry;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user.Id!, user);
            var resetUrl = baseUrl + "/api/Auth/reset-password?token=" + otpToken;
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

        public async Task<bool> Send2FACodeAsync(string email)
        {
            // Rate limiting: allow max 5 requests per hour per email
            var cacheKey = $"2fa-req:{email.ToLower()}";
            var countString = await _cache.GetStringAsync(cacheKey);
            int count = 0;
            if (!string.IsNullOrEmpty(countString))
                int.TryParse(countString, out count);

            int maxRequests = 5;
            if (count >= maxRequests)
            {
                // Optionally, implement exponential backoff by increasing expiry
                // For now, just return false if limit exceeded
                return false;
            }

            // Increment request count and set expiry (1 hour window)
            count++;
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            await _cache.SetStringAsync(cacheKey, count.ToString(), options);

            var user = await _userRepo.FindOneAsync(u => u.Email == email && u.IsEmailVerified);
            if (user == null) return false;
            // Generate secure 6-digit OTP
            int otpInt;
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                otpInt = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF; // ensure positive
                otpInt = otpInt % 900000 + 100000; // 6 digits
            }
            var otp = otpInt.ToString();
            user.OTPToken = otp;
            user.OTPExpiry = DateTime.UtcNow.AddMinutes(5);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user.Id!, user);
            var subject = "Mã xác thực 2 bước LocalMartOnline";
            var body = $"<p>Mã xác thực 2 bước của bạn là: <b>{otp}</b>. Mã có hiệu lực trong 5 phút.</p>";
            await _emailService.SendEmailAsync(user.Email, subject, body);
            return true;
        }

        public async Task<bool> Verify2FACodeAsync(string email, string otpCode)
        {
            var user = await _userRepo.FindOneAsync(u => u.Email == email && u.IsEmailVerified);
            if (user == null)
            {
                _logger.LogWarning("2FA verification failed: user not found or email not verified. Email: {Email}", email);
                return false;
            }
            if (user.OTPToken != otpCode)
            {
                _logger.LogWarning("2FA verification failed: invalid OTP for Email: {Email}", email);
                return false;
            }
            if (user.OTPExpiry == null || user.OTPExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("2FA verification failed: OTP expired for Email: {Email}", email);
                return false;
            }
            user.OTPToken = null;
            user.OTPExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user.Id!, user);
            _logger.LogInformation("2FA verification succeeded for UserId: {UserId}", user.Id);
            return true;
        }

        public async Task<UserDTO?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepo.FindOneAsync(u => u.Username == username);
            if (user == null) return null;
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            };
        }

        public async Task<UserDTO?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepo.FindOneAsync(u => u.Email == email);
            if (user == null) return null;
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            };
        }

        public async Task<UserDTO?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            var user = await _userRepo.FindOneAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
            if (user == null) return null;
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            };
        }

        public async Task<User?> GetUserEntityByEmailAsync(string email)
        {
            return await _userRepo.FindOneAsync(u => u.Email == email);
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

        public string GenerateJwtTokenFor2FA(User user)
        {
            return GenerateJwtToken(user);
        }
    }
}
