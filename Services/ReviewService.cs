using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Review;
using MongoDB.Driver;
using MongoDB.Bson;

namespace LocalMartOnline.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IMongoCollection<Review> _reviewCollection;
        private readonly IMongoCollection<Models.Order> _orderCollection;
        private readonly IMongoCollection<ProxyShoppingOrder> _proxyOrderCollection;        public ReviewService(IMongoDatabase database)
        {
            _reviewCollection = database.GetCollection<Review>("reviews");
            _orderCollection = database.GetCollection<Order>("orders");
            _proxyOrderCollection = database.GetCollection<ProxyShoppingOrder>("proxy_shopping_orders");
        }

        public async Task<ReviewDto?> CreateReviewAsync(string userId, CreateReviewDto createReviewDto)
        {
            // Validate rating
            if (createReviewDto.Rating < 1 || createReviewDto.Rating > 5)
                return null;

            // Check if user can review this target
            if (!await CanUserReviewAsync(userId, createReviewDto.TargetType, createReviewDto.TargetId))
                return null;

            // Check if user already reviewed this target
            var existingReview = await _reviewCollection
                .Find(r => r.UserId == userId && 
                          r.TargetType == createReviewDto.TargetType && 
                          r.TargetId == createReviewDto.TargetId)
                .FirstOrDefaultAsync();

            if (existingReview != null)
                return null; // User already reviewed this target

            var review = new Review
            {
                UserId = userId,
                TargetType = createReviewDto.TargetType,
                TargetId = createReviewDto.TargetId,
                Rating = createReviewDto.Rating,
                Comment = createReviewDto.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _reviewCollection.InsertOneAsync(review);

            return MapToDto(review);
        }

        public async Task<ReviewDto?> UpdateReviewAsync(string userId, string reviewId, UpdateReviewDto updateReviewDto)
        {
            // Validate rating
            if (updateReviewDto.Rating < 1 || updateReviewDto.Rating > 5)
                return null;

            var filter = Builders<Review>.Filter.And(
                Builders<Review>.Filter.Eq(r => r.Id, reviewId),
                Builders<Review>.Filter.Eq(r => r.UserId, userId)
            );

            var update = Builders<Review>.Update
                .Set(r => r.Rating, updateReviewDto.Rating)
                .Set(r => r.Comment, updateReviewDto.Comment)
                .Set(r => r.UpdatedAt, DateTime.UtcNow);

            var result = await _reviewCollection.FindOneAndUpdateAsync(
                filter, 
                update, 
                new FindOneAndUpdateOptions<Review> { ReturnDocument = ReturnDocument.After }
            );

            return result != null ? MapToDto(result) : null;
        }

        public async Task<bool> DeleteReviewAsync(string userId, string reviewId)
        {
            var filter = Builders<Review>.Filter.And(
                Builders<Review>.Filter.Eq(r => r.Id, reviewId),
                Builders<Review>.Filter.Eq(r => r.UserId, userId)
            );

            var result = await _reviewCollection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<GetReviewsResponseDto> GetReviewsByUserAsync(string userId)
        {
            var filter = Builders<Review>.Filter.Eq(r => r.UserId, userId);
            var reviews = await _reviewCollection
                .Find(filter)
                .SortByDescending(r => r.CreatedAt)
                .ToListAsync();

            var reviewDtos = reviews.Select(MapToDto).ToList();
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            return new GetReviewsResponseDto
            {
                Reviews = reviewDtos,
                TotalCount = reviews.Count,
                AverageRating = Math.Round(averageRating, 2)
            };
        }

        public async Task<GetReviewsResponseDto> GetReviewsForTargetAsync(string targetType, string targetId)
        {
            var filter = Builders<Review>.Filter.And(
                Builders<Review>.Filter.Eq(r => r.TargetType, targetType),
                Builders<Review>.Filter.Eq(r => r.TargetId, targetId)
            );

            var reviews = await _reviewCollection
                .Find(filter)
                .SortByDescending(r => r.CreatedAt)
                .ToListAsync();

            var reviewDtos = reviews.Select(MapToDto).ToList();
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            return new GetReviewsResponseDto
            {
                Reviews = reviewDtos,
                TotalCount = reviews.Count,
                AverageRating = Math.Round(averageRating, 2)
            };
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(string reviewId)
        {
            var review = await _reviewCollection
                .Find(r => r.Id == reviewId)
                .FirstOrDefaultAsync();

            return review != null ? MapToDto(review) : null;
        }

        public async Task<bool> CanUserReviewAsync(string userId, string targetType, string targetId)
        {
            // For now, we'll implement basic logic. In a real scenario, you'd check:
            // - For Product: User must have completed an order containing this product
            // - For Seller: User must have completed an order from this seller
            // - For ProxyShopper: User must have completed a proxy shopping order with this shopper

            switch (targetType.ToLower())
            {
                case "product":
                    // Check if user has completed orders with this product
                    return await HasCompletedOrderWithProductAsync(userId, targetId);
                
                case "seller":
                    // Check if user has completed orders from this seller
                    return await HasCompletedOrderFromSellerAsync(userId, targetId);
                
                case "proxyshopper":
                    // Check if user has completed proxy shopping orders with this shopper
                    return await HasCompletedProxyOrderWithShopperAsync(userId, targetId);
                
                default:
                    return false;
            }
        }        private async Task<bool> HasCompletedOrderWithProductAsync(string userId, string productId)
        {
            // This would require joining Orders with OrderItems to check if user has completed orders with this product
            // For now, return true to allow testing
            await Task.CompletedTask;
            return true;
        }

        private async Task<bool> HasCompletedOrderFromSellerAsync(string userId, string sellerId)
        {
            // This would require checking if user has completed orders from stores owned by this seller
            // For now, return true to allow testing
            await Task.CompletedTask;
            return true;
        }

        private async Task<bool> HasCompletedProxyOrderWithShopperAsync(string userId, string proxyShopperId)
        {
            // Check if user has completed proxy shopping orders with this shopper
            var filter = Builders<ProxyShoppingOrder>.Filter.And(
                Builders<ProxyShoppingOrder>.Filter.Eq(o => o.BuyerId, userId),
                Builders<ProxyShoppingOrder>.Filter.Eq(o => o.ProxyShopperId, proxyShopperId),
                Builders<ProxyShoppingOrder>.Filter.Eq(o => o.Status, "Completed")
            );

            var order = await _proxyOrderCollection.Find(filter).FirstOrDefaultAsync();
            return order != null;
        }

        private static ReviewDto MapToDto(Review review)
        {
            return new ReviewDto
            {
                Id = review.Id,
                UserId = review.UserId,
                TargetType = review.TargetType,
                TargetId = review.TargetId,
                Rating = review.Rating,
                Comment = review.Comment,
                Response = review.Response,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }
    }
}
