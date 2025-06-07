namespace LocalMartOnline.Models.DTOs
{
    public class NotificationRequestDTO
    {
        public required string UserToken { get; set; }
        public required string Message { get; set; }
    }

    public class NotificationConditionRequestDTO
    {
        public string? Role { get; set; }
        public string? Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
