using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace LocalMartOnline.Services.Implement
{
    public class MarketFeeService : IMarketFeeService
    {
        private readonly IRepository<MarketFee> _repo;
        private readonly IMapper _mapper;
        public MarketFeeService(IRepository<MarketFee> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MarketFeeDto>> GetAllAsync(string marketId)
        {
            var fees = await _repo.FindManyAsync(f => f.MarketId == marketId);
            return _mapper.Map<IEnumerable<MarketFeeDto>>(fees);
        }

        public async Task<MarketFeeDto?> GetByIdAsync(string id)
        {
            var fee = await _repo.GetByIdAsync(id);
            return fee == null ? null : _mapper.Map<MarketFeeDto>(fee);
        }

        public async Task<MarketFeeDto> CreateAsync(MarketFeeCreateDto dto)
        {
            var fee = _mapper.Map<MarketFee>(dto);
            fee.CreatedAt = DateTime.UtcNow;
            fee.UpdatedAt = DateTime.UtcNow;
            await _repo.CreateAsync(fee);
            return _mapper.Map<MarketFeeDto>(fee);
        }

        public async Task<MarketFeeDto> CreateAsync(MarketFeeDto dto)
        {
            // Map từ MarketFeeDto sang MarketFee entity
            var fee = _mapper.Map<MarketFee>(dto);
            fee.CreatedAt = DateTime.UtcNow;
            fee.UpdatedAt = DateTime.UtcNow;
            await _repo.CreateAsync(fee);
            return _mapper.Map<MarketFeeDto>(fee);
        }

        public async Task<bool> UpdateAsync(string id, MarketFeeUpdateDto dto)
        {
            var fee = await _repo.GetByIdAsync(id);
            if (fee == null) return false;
            if (dto.Name != null) fee.Name = dto.Name;
            if (dto.Amount != null) fee.Amount = dto.Amount.Value;
            if (dto.Description != null) fee.Description = dto.Description;
            fee.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(id, fee);
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var fee = await _repo.GetByIdAsync(id);
            if (fee == null) return false;
            await _repo.DeleteAsync(id);
            return true;
        }

        public Task<bool> PayFeeAsync(MarketFeePaymentDto dto)
        {
            // TODO: Tích hợp thanh toán online thực tế (VnPay, v.v.)
            // Ở đây chỉ giả lập đã thanh toán thành công
            return Task.FromResult(true);
        }
    }
}
