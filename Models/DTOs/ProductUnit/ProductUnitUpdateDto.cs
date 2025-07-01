using System.ComponentModel.DataAnnotations;
using LocalMartOnline.Models;

namespace LocalMartOnline.Models.DTOs.ProductUnit
{
    public class ProductUnitUpdateDto
    {
        [Required(ErrorMessage = "Tên đơn vị là bắt buộc")]
        [StringLength(20, ErrorMessage = "Tên đơn vị không được vượt quá 20 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên hiển thị là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên hiển thị không được vượt quá 50 ký tự")]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Mô tả không được vượt quá 200 ký tự")]
        public string Description { get; set; } = string.Empty;

        public bool RequiresIntegerQuantity { get; set; }

        [Required(ErrorMessage = "Loại đơn vị là bắt buộc")]
        public UnitType UnitType { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự sắp xếp phải >= 0")]
        public int SortOrder { get; set; }
    }
}