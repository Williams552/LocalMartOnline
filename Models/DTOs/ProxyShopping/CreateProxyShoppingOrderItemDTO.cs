namespace LocalMartOnline.Models.DTOs.ProxyShopping
{
    public class CreateProxyShoppingOrderItemDTO
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
