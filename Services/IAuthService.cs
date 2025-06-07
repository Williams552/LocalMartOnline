using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models;

namespace LocalMartOnline.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO?> LoginAsync(LoginRequestDTO loginDto);
        Task<string?> RegisterAsync(RegisterDTO registerDto);
        Task<bool> VerifyEmailAsync(string token);
        Task ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> Send2FACodeAsync(string email);
        Task<bool> Verify2FACodeAsync(string email, string otpCode);
        Task<UserDTO?> GetUserByUsernameAsync(string username);
        Task<UserDTO?> GetUserByEmailAsync(string email);
        Task<UserDTO?> GetUserByUsernameOrEmailAsync(string usernameOrEmail);
        string GenerateJwtTokenFor2FA(User user);
    }
}
