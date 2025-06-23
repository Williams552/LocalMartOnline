using LocalMartOnline.Models.DTOs.Category;
using LocalMartOnline.Models.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface ICategoryService
    {
        Task<PagedResultDto<CategoryDto>> GetAllPagedAsync(int page, int pageSize);
        Task<PagedResultDto<CategoryDto>> GetAllPagedAdminAsync(int page, int pageSize);
        Task<CategoryDto?> GetByIdAsync(string id);
        Task<CategoryDto> CreateAsync(CategoryCreateDto dto);
        Task<bool> UpdateAsync(string id, CategoryUpdateDto dto);
        Task<bool> DeleteAsync(string id);
        Task<bool> ToggleAsync(string id);
        Task<IEnumerable<CategoryDto>> SearchByNameAsync(string name);
        Task<IEnumerable<CategoryDto>> SearchByNameAdmin(string name);
        Task<IEnumerable<CategoryDto>> FilterByAlphabetAsync(char alphabet);
        Task<IEnumerable<CategoryDto>> FilterByAlphabetAdminAsync(char alphabet);
    }
}