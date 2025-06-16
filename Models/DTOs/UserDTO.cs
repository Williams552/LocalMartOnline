namespace LocalMartOnline.Models.DTOs
{
    public class UserDTO
    {
        public string? Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;

        public bool TwoFactorEnabled { get; set; }    }
}
