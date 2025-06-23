using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.CategoryRegistration;
using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Implement
{
    public class CategoryRegistrationService : ICategoryRegistrationService
    {
        private readonly IRepository<CategoryRegistration> _repo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IMapper _mapper;

        public CategoryRegistrationService(
            IRepository<CategoryRegistration> repo,
            IRepository<Category> categoryRepo,
            IMapper mapper)
        {
            _repo = repo;
            _categoryRepo = categoryRepo;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<CategoryRegistrationDto>> GetAllPagedAsync(int page, int pageSize)
        {
            var regs = await _repo.GetAllAsync();
            var total = regs.Count();
            var paged = regs.Skip((page - 1) * pageSize).Take(pageSize);
            var items = _mapper.Map<IEnumerable<CategoryRegistrationDto>>(paged);
            return new PagedResultDto<CategoryRegistrationDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> ApproveAsync(string id)
        {
            var reg = await _repo.GetByIdAsync(id);
            if (reg == null || reg.Status != CategoryRegistrationStatus.Pending) return false;

            reg.Status = CategoryRegistrationStatus.Approved;
            reg.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(id, reg);

            // Thêm category mới vào bảng Category
            var newCategory = new Category
            {
                Name = reg.CategoryName,
                Description = reg.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _categoryRepo.CreateAsync(newCategory);

            return true;
        }

        public async Task<bool> RejectAsync(string id, string rejectionReason)
        {
            var reg = await _repo.GetByIdAsync(id);
            if (reg == null || reg.Status != CategoryRegistrationStatus.Pending) return false;
            reg.Status = CategoryRegistrationStatus.Rejected;
            reg.RejectionReason = rejectionReason;
            reg.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(id, reg);
            return true;
        }

        public async Task<CategoryRegistrationDto> RegisterAsync(CategoryRegistrationCreateDto dto)
        {
            var entity = _mapper.Map<CategoryRegistration>(dto);
            entity.Status = CategoryRegistrationStatus.Pending;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.CreateAsync(entity);
            return _mapper.Map<CategoryRegistrationDto>(entity);
        }
    }
}