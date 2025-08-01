using LocalMartOnline.Models.DTOs.Review;

namespace LocalMartOnline.Services
{
    public interface IReviewService
    {
        Task<ReviewDto?> CreateReviewAsync(string userId, CreateReviewDto createReviewDto);
        Task<ReviewDto?> UpdateReviewAsync(string userId, string reviewId, UpdateReviewDto updateReviewDto);
        Task<ReviewDto?> UpdateReviewResponseAsync(string userId, string reviewId, UpdateReviewResponseDto updateResponseDto);
        Task<bool> DeleteReviewAsync(string userId, string reviewId);
        Task<GetReviewsResponseDto> GetReviewsByUserAsync(string userId);
        Task<GetReviewsResponseDto> GetReviewsForTargetAsync(string targetType, string targetId);
        Task<GetReviewsResponseDto> GetReviewsForTargetAsync(object filterOptions);
        Task<ReviewDto?> GetReviewByIdAsync(string reviewId);
        Task<bool> CanUserReviewAsync(string userId, string targetType, string targetId);
        Task<bool> IsOrderReviewedAsync(string userId, string orderId);
    }
}
