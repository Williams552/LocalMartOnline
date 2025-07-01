using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Models.DTOs.ProductUnit;

namespace LocalMartOnline.Services.Interface
{
    public interface IProductUnitService
    {
        Task<PagedResultDto<ProductUnitDto>> GetAllPagedAsync(int page, int pageSize);
        Task<List<ProductUnitDto>> GetActiveUnitsAsync();
        Task<ProductUnitDto> CreateAsync(ProductUnitCreateDto dto);
        Task<bool> UpdateAsync(string id, ProductUnitUpdateDto dto);
        Task<bool> ToggleAsync(string id);
        Task<bool> DeleteAsync(string id);
        Task<ProductUnitDto?> GetByIdAsync(string id);
        Task<List<ProductUnitDto>> SearchByNameAsync(string name);
        Task<List<ProductUnitDto>> SearchByNameAdminAsync(string name);
        Task<List<ProductUnitDto>> GetByUnitTypeAsync(UnitType unitType, bool activeOnly = true);
        Task<bool> ReorderUnitsAsync(List<ProductUnitReorderDto> reorderList);
        Task<bool> IsNameExistsAsync(string name, string? excludeId = null);
    }
}