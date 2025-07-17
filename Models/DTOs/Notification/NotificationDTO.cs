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

    public class StoreSuspensionNotificationDto
    {
        public string UserToken { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class NotificationDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}
