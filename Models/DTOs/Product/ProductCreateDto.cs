using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LocalMartOnline.Models.DTOs.Product
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Store ID là bắt buộc")]
        public string StoreId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category ID là bắt buộc")]
        public string CategoryId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Đơn vị đo lường là bắt buộc")]
        public string UnitId { get; set; } = string.Empty; // Reference to ProductUnit

        [Range(0.01, double.MaxValue, ErrorMessage = "Số lượng tối thiểu phải lớn hơn 0")]
        public decimal MinimumQuantity { get; set; } = 1;

        public List<string> ImageUrls { get; set; } = new();
    }
}