namespace LocalMartOnline.Models.DTOs.PlatformPolicy
{
    public class PlatformPolicyDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class PlatformPolicyCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class PlatformPolicyUpdateDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }

    public class PlatformPolicyFilterDto
    {
        public bool? IsActive { get; set; }
    }
}