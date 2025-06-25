using System.ComponentModel.DataAnnotations;

namespace LocalMartOnline.Models.DTOs.SupportRequest
{
    public class SupportRequestDto
    {
        public string? Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Response { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateSupportRequestDto
    {
        [Required(ErrorMessage = "Subject is required")]
        [StringLength(100, ErrorMessage = "Subject cannot exceed 100 characters")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;
    }

    public class RespondToSupportRequestDto
    {
        [Required(ErrorMessage = "Response is required")]
        public string Response { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty; // InProgress or Resolved
    }

    public class UpdateSupportRequestStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
