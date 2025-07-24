using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Models.DTOs.ProductUnit;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Implement
{
    public class ProductUnitService : IProductUnitService
    {
        private readonly IRepository<ProductUnit> _unitRepo;
        private readonly IMapper _mapper;

        public ProductUnitService(IRepository<ProductUnit> unitRepo, IMapper mapper)
        {
            _unitRepo = unitRepo;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<ProductUnitDto>> GetAllPagedAsync(int page, int pageSize)
        {
            var units = await _unitRepo.GetAllAsync();
            var sortedUnits = units.OrderBy(u => u.SortOrder).ThenBy(u => u.Name).ToList();
            
            var total = sortedUnits.Count();
            var paged = sortedUnits.Skip((page - 1) * pageSize).Take(pageSize);
            var items = _mapper.Map<IEnumerable<ProductUnitDto>>(paged);
            
            return new PagedResultDto<ProductUnitDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<ProductUnitDto>> GetActiveUnitsAsync()
        {
            var units = await _unitRepo.FindManyAsync(u => u.IsActive);
            var sortedUnits = units.OrderBy(u => u.SortOrder).ThenBy(u => u.Name);
            return _mapper.Map<List<ProductUnitDto>>(sortedUnits);
        }

        public async Task<ProductUnitDto> CreateAsync(ProductUnitCreateDto dto)
        {
            var unit = _mapper.Map<ProductUnit>(dto);
            unit.CreatedAt = DateTime.Now;
            unit.UpdatedAt = DateTime.Now;
            unit.IsActive = true;
            
            await _unitRepo.CreateAsync(unit);
            return _mapper.Map<ProductUnitDto>(unit);
        }

        public async Task<bool> UpdateAsync(string id, ProductUnitUpdateDto dto)
        {
            var unit = await _unitRepo.GetByIdAsync(id);
            if (unit == null) return false;
            
            _mapper.Map(dto, unit);
            unit.UpdatedAt = DateTime.Now;
            
            await _unitRepo.UpdateAsync(id, unit);
            return true;
        }

        public async Task<bool> ToggleAsync(string id)
        {
            var unit = await _unitRepo.GetByIdAsync(id);
            if (unit == null) return false;
            
            unit.IsActive = !unit.IsActive;
            unit.UpdatedAt = DateTime.Now;
            
            await _unitRepo.UpdateAsync(id, unit);
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var unit = await _unitRepo.GetByIdAsync(id);
            if (unit == null) return false;
            
            // Soft delete - set to inactive
            unit.IsActive = false;
            unit.UpdatedAt = DateTime.Now;
            
            await _unitRepo.UpdateAsync(id, unit);
            return true;
        }

        public async Task<ProductUnitDto?> GetByIdAsync(string id)
        {
            var unit = await _unitRepo.GetByIdAsync(id);
            if (unit == null) return null;
            
            return _mapper.Map<ProductUnitDto>(unit);
        }

        public async Task<List<ProductUnitDto>> SearchByNameAsync(string name)
        {
            var units = await _unitRepo.FindManyAsync(u => 
                u.IsActive && 
                (u.Name.Contains(name, StringComparison.OrdinalIgnoreCase) || 
                 u.DisplayName.Contains(name, StringComparison.OrdinalIgnoreCase))
            );
            
            var sortedUnits = units.OrderBy(u => u.SortOrder).ThenBy(u => u.Name);
            return _mapper.Map<List<ProductUnitDto>>(sortedUnits);
        }

        public async Task<List<ProductUnitDto>> SearchByNameAdminAsync(string name)
        {
            var units = await _unitRepo.FindManyAsync(u => 
                u.Name.Contains(name, StringComparison.OrdinalIgnoreCase) || 
                u.DisplayName.Contains(name, StringComparison.OrdinalIgnoreCase)
            );
            
            var sortedUnits = units.OrderBy(u => u.SortOrder).ThenBy(u => u.Name);
            return _mapper.Map<List<ProductUnitDto>>(sortedUnits);
        }

        public async Task<List<ProductUnitDto>> GetByUnitTypeAsync(UnitType unitType, bool activeOnly = true)
        {
            var units = activeOnly 
                ? await _unitRepo.FindManyAsync(u => u.UnitType == unitType && u.IsActive)
                : await _unitRepo.FindManyAsync(u => u.UnitType == unitType);
            
            var sortedUnits = units.OrderBy(u => u.SortOrder).ThenBy(u => u.Name);
            return _mapper.Map<List<ProductUnitDto>>(sortedUnits);
        }

        public async Task<bool> ReorderUnitsAsync(List<ProductUnitReorderDto> reorderList)
        {
            foreach (var item in reorderList)
            {
                var unit = await _unitRepo.GetByIdAsync(item.Id);
                if (unit != null)
                {
                    unit.SortOrder = item.SortOrder;
                    unit.UpdatedAt = DateTime.Now;
                    await _unitRepo.UpdateAsync(item.Id, unit);
                }
            }
            return true;
        }

        public async Task<bool> IsNameExistsAsync(string name, string? excludeId = null)
        {
            var units = string.IsNullOrEmpty(excludeId)
                ? await _unitRepo.FindManyAsync(u => u.Name.ToLower() == name.ToLower())
                : await _unitRepo.FindManyAsync(u => u.Name.ToLower() == name.ToLower() && u.Id != excludeId);
                
            return units.Any();
        }
    }
}