namespace LocalMartOnline.Models.DTOs.Common
{
    public class PagedRequestDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}