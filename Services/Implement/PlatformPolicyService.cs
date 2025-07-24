using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.PlatformPolicy;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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

        public async Task<IEnumerable<PlatformPolicyDto>> GetAllAsync()
        {
            var policies = await _policyRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<PlatformPolicyDto>>(policies);
        }

        public async Task<PlatformPolicyDto?> GetByIdAsync(string id)
        {
            var policy = await _policyRepo.GetByIdAsync(id);
            return policy == null ? null : _mapper.Map<PlatformPolicyDto>(policy);
        }

        public async Task<bool> UpdateAsync(string id, PlatformPolicyUpdateDto dto)
        {
            var policy = await _policyRepo.GetByIdAsync(id);
            if (policy == null) return false;
            _mapper.Map(dto, policy);
            policy.UpdatedAt = DateTime.Now;
            await _policyRepo.UpdateAsync(id, policy);
            return true;
        }
    }
}