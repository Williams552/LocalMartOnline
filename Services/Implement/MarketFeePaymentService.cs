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
    public class MarketFeePaymentService : IMarketFeePaymentService
    {
        private readonly IRepository<MarketFeePayment> _repo;
        private readonly IMapper _mapper;
        public MarketFeePaymentService(IRepository<MarketFeePayment> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MarketFeePaymentDto>> GetPaymentsBySellerAsync(long sellerId)
        {
            var payments = await _repo.FindManyAsync(p => p.SellerId == sellerId);
            return _mapper.Map<IEnumerable<MarketFeePaymentDto>>(payments);
        }

        public async Task<MarketFeePaymentDto?> GetPaymentByIdAsync(long paymentId)
        {
            var payment = await _repo.GetByIdAsync(paymentId.ToString());
            return payment == null ? null : _mapper.Map<MarketFeePaymentDto>(payment);
        }

        public async Task<MarketFeePaymentDto> CreatePaymentAsync(MarketFeePaymentCreateDto dto)
        {
            var payment = _mapper.Map<MarketFeePayment>(dto);
            payment.PaymentStatus = MarketFeePaymentStatus.Pending;
            payment.CreatedAt = DateTime.Now;
            await _repo.CreateAsync(payment);
            return _mapper.Map<MarketFeePaymentDto>(payment);
        }

        public async Task<bool> UpdatePaymentStatusAsync(long paymentId, string status)
        {
            var payment = await _repo.GetByIdAsync(paymentId.ToString());
            if (payment == null) return false;
            if (Enum.TryParse<MarketFeePaymentStatus>(status, out var newStatus))
            {
                payment.PaymentStatus = newStatus;
                await _repo.UpdateAsync(paymentId.ToString(), payment);
                return true;
            }
            return false;
        }
    }
}
