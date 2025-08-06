using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.PlatformPolicy;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace LocalMartOnline.Services.Implement
{
    public class PlatformPolicyService : IPlatformPolicyService
    {
        private readonly IRepository<PlatformPolicy> _policyRepo;
        private readonly IMapper _mapper;

        public PlatformPolicyService(IRepository<PlatformPolicy> policyRepo, IMapper mapper)
        {
            _policyRepo = policyRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PlatformPolicyDto>> GetAllAsync(PlatformPolicyFilterDto? filter = null)
        {
            var policies = await _policyRepo.GetAllAsync();
            
            // Apply filter if provided
            if (filter?.IsActive.HasValue == true)
            {
                policies = policies.Where(p => p.IsActive == filter.IsActive.Value);
            }
            
            return _mapper.Map<IEnumerable<PlatformPolicyDto>>(policies);
        }

        public async Task<PlatformPolicyDto?> GetByIdAsync(string id)
        {
            var policy = await _policyRepo.GetByIdAsync(id);
            return policy == null ? null : _mapper.Map<PlatformPolicyDto>(policy);
        }

        public async Task<PlatformPolicyDto> CreateAsync(PlatformPolicyCreateDto dto)
        {
            var policy = _mapper.Map<PlatformPolicy>(dto);
            policy.CreatedAt = DateTime.Now;
            policy.UpdatedAt = DateTime.Now;
            await _policyRepo.CreateAsync(policy);
            return _mapper.Map<PlatformPolicyDto>(policy);
        }

        public async Task<bool> UpdateAsync(string id, PlatformPolicyUpdateDto dto)
        {
            var policy = await _policyRepo.GetByIdAsync(id);
            if (policy == null) return false;
            
            // Chỉ cập nhật các field được truyền vào (không null)
            if (!string.IsNullOrEmpty(dto.Title))
                policy.Title = dto.Title;
            
            if (!string.IsNullOrEmpty(dto.Content))
                policy.Content = dto.Content;
            
            if (dto.IsActive.HasValue)
                policy.IsActive = dto.IsActive.Value;
            
            policy.UpdatedAt = DateTime.Now;
            await _policyRepo.UpdateAsync(id, policy);
            return true;
        }

        public async Task<bool> ToggleAsync(string id)
        {
            var policy = await _policyRepo.GetByIdAsync(id);
            if (policy == null) return false;
            else if(policy.IsActive == true) policy.IsActive = false;
            else if(policy.IsActive == false) policy.IsActive = true; // Already deactivated
            policy.UpdatedAt = DateTime.Now;
            await _policyRepo.UpdateAsync(id, policy);
            return true;
        }
    }
}