using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using LocalMartOnline.Models;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Models.DTOs.Category;
using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Services.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IProductService _productService;

        public CategoryService(IMongoDatabase database, IProductService productService)
        {
            _categoryCollection = database.GetCollection<Category>("Categories");
            _productService = productService; 
        }

        public async Task<GetCategoriesResponseDto> GetActiveCategoriesAsync()
        {
            var filter = Builders<Category>.Filter.Eq(c => c.IsActive, true);
            var sort = Builders<Category>.Sort.Ascending(c => c.Name);

            var categories = await _categoryCollection
                .Find(filter)
                .Sort(sort)
                .ToListAsync();

            var categoryDtos = categories.Select(c => new CategoryDto
            {
                CategoryId = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive
            }).ToList();

            return new GetCategoriesResponseDto
            {
                Categories = categoryDtos
            };
        }

        public async Task<SearchProductResultDto> GetProductsByCategoryAsync(string categoryId, int page = 1, int pageSize = 20, string sortPrice = "")
        {
            var request = new SearchProductRequestDto
            {
                CategoryId = categoryId,
                Page = page,
                PageSize = pageSize,
                SortPrice = sortPrice
            };
            
            return await _productService.SearchProductsAsync(request);
        }
    }
}