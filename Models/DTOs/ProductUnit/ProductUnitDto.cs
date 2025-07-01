using LocalMartOnline.Models;

namespace LocalMartOnline.Models.DTOs.ProductUnit
{
    public class ProductUnitDto
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool RequiresIntegerQuantity { get; set; }
        public UnitType UnitType { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductUnitCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool RequiresIntegerQuantity { get; set; } = false;
        public UnitType UnitType { get; set; } = UnitType.Count;
        public int SortOrder { get; set; } = 0;
    }

}