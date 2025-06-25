using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Favorite;
using MongoDB.Driver;
using MongoDB.Bson;

namespace LocalMartOnline.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IMongoCollection<Favorite> _favoriteCollection;
        private readonly IMongoCollection<Product> _productCollection;

        public FavoriteService(IMongoDatabase database)
        {
            _favoriteCollection = database.GetCollection<Favorite>("favorites");
            _productCollection = database.GetCollection<Product>("products");
        }

        public async Task<FavoriteActionResponseDto> AddToFavoriteAsync(string userId, string productId)
        {
            try
            {
                // Check if already in favorites
                var existingFavorite = await _favoriteCollection
                    .Find(f => f.UserId == userId && f.ProductId == productId)
                    .FirstOrDefaultAsync();

                if (existingFavorite != null)
                {
                    return new FavoriteActionResponseDto
                    {
                        Success = false,
                        Message = "Product is already in your favorites"
                    };
                }

                // Check if product exists and is active
                var product = await _productCollection
                    .Find(p => p.Id == productId && p.Status == ProductStatus.Active)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return new FavoriteActionResponseDto
                    {
                        Success = false,
                        Message = "Product not found or inactive"
                    };
                }

                var favorite = new Favorite
                {
                    UserId = userId,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                };

                await _favoriteCollection.InsertOneAsync(favorite);

                return new FavoriteActionResponseDto
                {
                    Success = true,
                    Message = "Product added to favorites successfully"
                };
            }
            catch (Exception ex)
            {
                return new FavoriteActionResponseDto
                {
                    Success = false,
                    Message = $"Error adding to favorites: {ex.Message}"
                };
            }
        }

        public async Task<GetFavoriteProductsResponseDto> GetUserFavoriteProductsAsync(string userId, int page = 1, int pageSize = 20)
        {
            var pipeline = new List<BsonDocument>
            {
                // Match user's favorites
                new BsonDocument("$match", new BsonDocument("user_id", new ObjectId(userId))),
                
                // Lookup product information
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "products" },
                    { "localField", "product_id" },
                    { "foreignField", "_id" },
                    { "as", "product" }
                }),
                
                // Unwind product array
                new BsonDocument("$unwind", "$product"),
                
                // Match only active products
                new BsonDocument("$match", new BsonDocument("product.status", "Active")),
                
                // Lookup store information
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "stores" },
                    { "localField", "product.store_id" },
                    { "foreignField", "_id" },
                    { "as", "store" }
                }),
                
                // Unwind store array
                new BsonDocument("$unwind", "$store"),
                
                // Lookup category information
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "categories" },
                    { "localField", "product.category_id" },
                    { "foreignField", "_id" },
                    { "as", "category" }
                }),
                
                // Unwind category array
                new BsonDocument("$unwind", "$category"),
                
                // Lookup product images
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "product_images" },
                    { "localField", "product._id" },
                    { "foreignField", "product_id" },
                    { "as", "images" }
                }),
                
                // Sort by most recently added to favorites
                new BsonDocument("$sort", new BsonDocument("created_at", -1))
            };

            // Get total count
            var countPipeline = new List<BsonDocument>(pipeline);
            countPipeline.Add(new BsonDocument("$count", "total"));
            
            var countResult = await _favoriteCollection.Aggregate<BsonDocument>(countPipeline).FirstOrDefaultAsync();
            var totalCount = countResult?.GetValue("total", 0).AsInt32 ?? 0;

            // Add pagination
            pipeline.Add(new BsonDocument("$skip", (page - 1) * pageSize));
            pipeline.Add(new BsonDocument("$limit", pageSize));

            // Project final result
            pipeline.Add(new BsonDocument("$project", new BsonDocument
            {
                { "favorite_id", "$_id" },
                { "product_id", "$product._id" },
                { "product_name", "$product.name" },
                { "description", "$product.description" },
                { "price", "$product.price" },
                { "stock_quantity", "$product.stock_quantity" },
                { "status", "$product.status" },
                { "category_name", "$category.name" },
                { "store_name", "$store.name" },
                { "store_id", "$store._id" },
                { "image_url", new BsonDocument("$arrayElemAt", new BsonArray { "$images.image_url", 0 }) },
                { "added_to_favorite_at", "$created_at" }
            }));

            var favorites = await _favoriteCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var favoriteProducts = favorites.Select(doc => new FavoriteProductDto
            {
                FavoriteId = doc.GetValue("favorite_id").AsObjectId.ToString(),
                ProductId = doc.GetValue("product_id").AsObjectId.ToString(),
                ProductName = doc.GetValue("product_name").AsString,
                Description = doc.Contains("description") ? doc.GetValue("description").AsString : null,
                Price = doc.GetValue("price").AsDecimal,
                StockQuantity = doc.GetValue("stock_quantity").AsInt32,
                Status = doc.GetValue("status").AsString,
                CategoryName = doc.GetValue("category_name").AsString,
                StoreName = doc.GetValue("store_name").AsString,
                StoreId = doc.GetValue("store_id").AsObjectId.ToString(),
                ImageUrl = doc.Contains("image_url") && !doc.GetValue("image_url").IsBsonNull ? 
                          doc.GetValue("image_url").AsString : null,
                AddedToFavoriteAt = doc.GetValue("added_to_favorite_at").ToUniversalTime()
            }).ToList();

            return new GetFavoriteProductsResponseDto
            {
                FavoriteProducts = favoriteProducts,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<FavoriteActionResponseDto> RemoveFromFavoriteAsync(string userId, string productId)
        {
            try
            {
                var result = await _favoriteCollection.DeleteOneAsync(
                    f => f.UserId == userId && f.ProductId == productId);

                if (result.DeletedCount == 0)
                {
                    return new FavoriteActionResponseDto
                    {
                        Success = false,
                        Message = "Product not found in favorites"
                    };
                }

                return new FavoriteActionResponseDto
                {
                    Success = true,
                    Message = "Product removed from favorites successfully"
                };
            }
            catch (Exception ex)
            {
                return new FavoriteActionResponseDto
                {
                    Success = false,
                    Message = $"Error removing from favorites: {ex.Message}"
                };
            }
        }

        public async Task<bool> IsProductInFavoriteAsync(string userId, string productId)
        {
            var favorite = await _favoriteCollection
                .Find(f => f.UserId == userId && f.ProductId == productId)
                .FirstOrDefaultAsync();

            return favorite != null;
        }
    }
}
