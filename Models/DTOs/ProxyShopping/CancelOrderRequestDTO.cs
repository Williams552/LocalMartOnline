namespace LocalMartOnline.Models.DTOs.ProxyShopping
{
    // DTO cho upload ảnh + ghi chú chứng từ mua hàng
    public class UploadBoughtItemsDTO
    {
        public List<string> ImageUrls { get; set; } = new();
        public string? Note { get; set; }
    }

    // DTO cho huỷ đơn (ProxyShopper gửi lý do huỷ)
    public class CancelOrderDTO
    {
        public string Reason { get; set; } = string.Empty;
    }
}
