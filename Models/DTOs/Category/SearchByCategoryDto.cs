namespace LocalMartOnline.Models.DTOs.Category
{
    // Sử dụng CategoryDto từ CategoryDto.cs
    public class GetCategoriesResponseDto
    {
        public List<CategoryDto> Categories { get; set; } = new();
    }
}
