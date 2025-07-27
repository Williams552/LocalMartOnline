namespace LocalMartOnline.Models.DTOs
{
    public class RevenueAnalyticsDto
    {
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public string Period { get; set; }
    }

    public class OrderAnalyticsDto
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public string Period { get; set; }
    }

    public class CategoryAnalyticsDto
    {
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ProductAnalyticsDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int SoldQuantity { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; } // Số lượng đơn hàng chứa sản phẩm
        public double AverageRating { get; set; } // Đánh giá trung bình
    }
}
