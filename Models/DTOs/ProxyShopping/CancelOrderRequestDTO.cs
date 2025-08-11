namespace LocalMartOnline.Models.DTOs.ProxyShopping
{
    // DTO cho upload ảnh + ghi chú chứng từ mua hàng
    public class UploadBoughtItemsDTO
    {
        public string ImageUrls { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    // DTO cho huỷ đơn (ProxyShopper gửi lý do huỷ)
    public class CancelOrderDTO
    {
        public string Reason { get; set; } = string.Empty;
    }

    // DTO cho từ chối đề xuất (Buyer gửi lý do từ chối)
    public class RejectProposalDTO
    {
        public string Reason { get; set; } = string.Empty;
    }
}
