using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Services
{
    public class ProductService : IProductService
    {
        private readonly IMongoCollection<BsonDocument> _productCollection;

        public ProductService(IMongoDatabase database)
        {
            _productCollection = database.GetCollection<BsonDocument>("products");
        }

        public async Task<SearchProductResultDto> SearchProductsAsync(SearchProductRequestDto request)
        {
            var pipeline = new List<BsonDocument>();

            // Match active products with open stores
            var matchStage = new BsonDocument("$match", new BsonDocument
            {
                { "status", "Active" }
            });
            pipeline.Add(matchStage);

            // Lookup store information
            pipeline.Add(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "stores" },
                { "localField", "store_id" },
                { "foreignField", "_id" },
                { "as", "store" }
            }));

            // Unwind store array
            pipeline.Add(new BsonDocument("$unwind", "$store"));

            // Match only open stores
            pipeline.Add(new BsonDocument("$match", new BsonDocument
            {
                { "store.status", "Open" }
            }));

            // Lookup category information
            pipeline.Add(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "categories" },
                { "localField", "category_id" },
                { "foreignField", "_id" },
                { "as", "category" }
            }));

            // Unwind category array
            pipeline.Add(new BsonDocument("$unwind", "$category"));

            // Match only active categories
            pipeline.Add(new BsonDocument("$match", new BsonDocument
            {
                { "category.isActive", true }
            }));

            // Lookup product images
            pipeline.Add(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "product_images" },
                { "localField", "_id" },
                { "foreignField", "product_id" },
                { "as", "images" }
            }));

            // Add search filter if provided
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                pipeline.Add(new BsonDocument("$match", new BsonDocument
                {
                    { "name", new BsonDocument("$regex", request.Search)
                        .Add("$options", "i") } // Case insensitive
                }));
            }

            // Add category filter if provided
            if (!string.IsNullOrWhiteSpace(request.CategoryId))
            {
                pipeline.Add(new BsonDocument("$match", new BsonDocument
                {
                    { "category_id", new ObjectId(request.CategoryId) }
                }));
            }

            // Add projection to format response
            pipeline.Add(new BsonDocument("$project", new BsonDocument
            {
                { "product_id", "$_id" },
                { "name", 1 },
                { "description", 1 },
                { "price", 1 },
                { "stock_quantity", 1 },
                { "status", 1 },
                { "category_name", "$category.name" },
                { "store_name", "$store.name" },
                { "store_id", "$store._id" },
                { "image_url", new BsonDocument("$arrayElemAt", new BsonArray { "$images.image_url", 0 }) }
            }));

            // Add sorting
            if (!string.IsNullOrWhiteSpace(request.SortPrice))
            {
                var sortDirection = request.SortPrice.ToLower() == "desc" ? -1 : 1;
                pipeline.Add(new BsonDocument("$sort", new BsonDocument("price", sortDirection)));
            }
            else
            {
                pipeline.Add(new BsonDocument("$sort", new BsonDocument("name", 1)));
            }

            // Get total count
            var countPipeline = new List<BsonDocument>(pipeline);
            countPipeline.Add(new BsonDocument("$count", "total"));
            
            var countResult = await _productCollection.Aggregate<BsonDocument>(countPipeline).FirstOrDefaultAsync();
            var totalCount = countResult?.GetValue("total", 0).AsInt32 ?? 0;

            // Add pagination
            pipeline.Add(new BsonDocument("$skip", (request.Page - 1) * request.PageSize));
            pipeline.Add(new BsonDocument("$limit", request.PageSize));

            var products = await _productCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var productList = products.Select(doc => new SearchProductResponseDto
            {
                ProductId = doc.GetValue("product_id").AsObjectId.ToString(),
                Name = doc.GetValue("name").AsString,
                Description = doc.Contains("description") ? doc.GetValue("description").AsString : null,
                Price = doc.GetValue("price").AsDecimal,
                StockQuantity = doc.GetValue("stock_quantity").AsInt32,
                Status = doc.GetValue("status").AsString,
                CategoryName = doc.GetValue("category_name").AsString,
                StoreName = doc.GetValue("store_name").AsString,
                StoreId = doc.GetValue("store_id").AsObjectId.ToString(),
                ImageUrl = doc.Contains("image_url") && !doc.GetValue("image_url").IsBsonNull ? 
                          doc.GetValue("image_url").AsString : null
            }).ToList();

            return new SearchProductResultDto
            {
                Products = productList,
                TotalCount = totalCount,
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }
    }
}