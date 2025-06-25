namespace LocalMartOnline.Models.DTOs.Review
{
    public class CreateReviewDto
    {
        public string TargetType { get; set; } = string.Empty; // 'Product', 'Seller', 'ProxyShopper'
        public string TargetId { get; set; } = string.Empty;
        public int Rating { get; set; } // 1 to 5
        public string? Comment { get; set; }
    }

    public class UpdateReviewDto
    {
        public int Rating { get; set; } // 1 to 5
        public string? Comment { get; set; }
    }

    public class ReviewDto
    {
        public string? Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? Response { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class GetReviewsResponseDto
    {
        public List<ReviewDto> Reviews { get; set; } = new();
        public int TotalCount { get; set; }
        public double AverageRating { get; set; }
    }
}
