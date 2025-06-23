using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Category;
using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(IRepository<Category> categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<CategoryDto>> GetAllPagedAsync(int page, int pageSize)
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
            var categories = await _categoryRepository.FindManyAsync(c => c.Name.ToLower().Contains(name.ToLower()));
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> FilterByAlphabetAsync(char alphabet)
        {
            var categories = await _categoryRepository.FindManyAsync(c => c.Name.StartsWith(alphabet.ToString(), StringComparison.OrdinalIgnoreCase));
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }
    }
}