namespace LocalMartOnline.Models.DTOs.User
{
    public class UserUpdateDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; } = null;
        public string? Role { get; set; } = null; // Admin có thể update role
        public string? Status { get; set; } = null; // Admin có thể update status
        public bool? TwoFactorEnabled { get; set; } = null;
    }
}
