namespace LocalMartOnline.Models.DTOs
{
    public class NotificationRequestDTO
    {
        public string? UserToken { get; set; }
        public string? Message { get; set; }
    }

    public class NotificationConditionRequestDTO
    {
        public string? Role { get; set; }
        public string? Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
