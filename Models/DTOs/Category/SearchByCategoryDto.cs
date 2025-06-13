namespace LocalMartOnline.Models.DTOs.Category
{
    public class CategoryDto
    {
        public string CategoryId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class GetCategoriesResponseDto
    {
        public List<CategoryDto> Categories { get; set; } = new();
    }
}
