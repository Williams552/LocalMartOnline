using System;

namespace LocalMartOnline.Models.DTOs.CategoryRegistration
{
    public class CategoryRegistrationDto
    {
        public string? Id { get; set; }
        public string? SellerId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}