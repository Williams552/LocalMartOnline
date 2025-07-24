using System.ComponentModel.DataAnnotations;

namespace LocalMartOnline.Models.DTOs.Order
{
    public class OrderCancelDto
    {
        [Required(ErrorMessage = "Lý do hủy đơn hàng là bắt buộc")]
        [MinLength(10, ErrorMessage = "Lý do hủy phải có ít nhất 10 ký tự")]
        [MaxLength(500, ErrorMessage = "Lý do hủy không được quá 500 ký tự")]
        public string CancelReason { get; set; } = string.Empty;
    }
}
