using System.ComponentModel.DataAnnotations;

namespace LocalMartOnline.Models.DTOs.ProductUnit
{
    public class ProductUnitReorderDto
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int SortOrder { get; set; }
    }
}