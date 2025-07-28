namespace LocalMartOnline.Models.DTOs.Shopper
{
    public class ShopperAnalyticsDto
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public double CompletedRate
        {
            get
            {
                return TotalOrders > 0 ? (double)CompletedOrders / TotalOrders * 100 : 0;
            }
        }
        public decimal TotalSpent { get; set; }
        public double AverageRating { get; set; }
    }
}