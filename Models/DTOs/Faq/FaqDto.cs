namespace LocalMartOnline.Models.DTOs.Faq
{
    public class FaqDto
    {
        public string? Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }
}