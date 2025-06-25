using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Category;
using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Models.DTOs.Product;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IProductService _productService;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(IMongoDatabase database, IProductService productService, IRepository<Category> categoryRepository, IMapper mapper)
        {
            _categoryCollection = database.GetCollection<Category>("Categories");
            _productService = productService;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<GetCategoriesResponseDto> GetActiveCategoriesAsync()
        {
            var filter = Builders<Category>.Filter.Eq(c => c.IsActive, true);
            var sort = Builders<Category>.Sort.Ascending(c => c.Name);
            var categories = await _categoryCollection.Find(filter).Sort(sort).ToListAsync();
            var categoryDtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive
            }).ToList();
            return new GetCategoriesResponseDto { Categories = categoryDtos };
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

        public async Task<PagedResultDto<CategoryDto>> GetAllPagedAsync(int page, int pageSize)
        {
            var categories = await _categoryRepository.GetAllAsync();
            var activeCategories = categories.Where(c => c.IsActive == true);
            var total = activeCategories.Count();
            var paged = activeCategories.Skip((page - 1) * pageSize).Take(pageSize);
            var items = _mapper.Map<IEnumerable<CategoryDto>>(paged);
            return new PagedResultDto<CategoryDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultDto<CategoryDto>> GetAllPagedAdminAsync(int page, int pageSize)
        {
            var categories = await _categoryRepository.GetAllAsync();
            var total = categories.Count();
            var paged = categories.Skip((page - 1) * pageSize).Take(pageSize);
            var items = _mapper.Map<IEnumerable<CategoryDto>>(paged);
            return new PagedResultDto<CategoryDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CategoryDto?> GetByIdAsync(string id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category == null ? null : _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            await _categoryRepository.CreateAsync(category);
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> UpdateAsync(string id, CategoryUpdateDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;
            _mapper.Map(dto, category);
            category.UpdatedAt = DateTime.UtcNow;
            await _categoryRepository.UpdateAsync(id, category);
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;
            await _categoryRepository.DeleteAsync(id);
            return true;
        }
        public async Task<bool> ToggleAsync(string id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;
            category.IsActive = !category.IsActive;
            category.UpdatedAt = DateTime.UtcNow;
            await _categoryRepository.UpdateAsync(id, category);
            return true;
        }

        public async Task<IEnumerable<CategoryDto>> SearchByNameAsync(string name)
        {
            var categories = await _categoryRepository.FindManyAsync(c => c.Name.ToLower().Contains(name.ToLower()) && c.IsActive == true);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> SearchByNameAdmin(string name)
        {
            var categories = await _categoryRepository.FindManyAsync(c => c.Name.ToLower().Contains(name.ToLower()));
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> FilterByAlphabetAsync(char alphabet)
        {
            var upperChar = char.ToUpper(alphabet);
            var lowerChar = char.ToLower(alphabet);

            // Tạo filter cho cả uppercase và lowercase
            var categories = await _categoryRepository.FindManyAsync(c =>
                c.Name.StartsWith(upperChar.ToString()) && 
                c.IsActive == true ||
                c.Name.StartsWith(lowerChar.ToString()) &&
                c.IsActive == true);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> FilterByAlphabetAdminAsync(char alphabet)
        {
            var upperChar = char.ToUpper(alphabet);
            var lowerChar = char.ToLower(alphabet);

            // Tạo filter cho cả uppercase và lowercase
            var categories = await _categoryRepository.FindManyAsync(c =>
                c.Name.StartsWith(upperChar.ToString()) ||
                c.Name.StartsWith(lowerChar.ToString()));
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }
    }
}