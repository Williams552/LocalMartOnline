using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Faq;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace LocalMartOnline.Services.Implement
{
    public class FaqService : IFaqService
    {
        private readonly IRepository<Faq> _faqRepo;
        private readonly IMapper _mapper;

        public FaqService(IRepository<Faq> faqRepo, IMapper mapper)
        {
            _faqRepo = faqRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FaqDto>> GetAllAsync()
        {
            var faqs = await _faqRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<FaqDto>>(faqs);
        }

        public async Task<FaqDto?> GetByIdAsync(string id)
        {
            var faq = await _faqRepo.GetByIdAsync(id);
            return faq == null ? null : _mapper.Map<FaqDto>(faq);
        }

        public async Task<FaqDto> AddAsync(FaqCreateDto dto)
        {
            var faq = _mapper.Map<Faq>(dto);
            faq.CreatedAt = DateTime.UtcNow;
            faq.UpdatedAt = DateTime.UtcNow;
            await _faqRepo.CreateAsync(faq);
            return _mapper.Map<FaqDto>(faq);
        }

        public async Task<bool> UpdateAsync(string id, FaqUpdateDto dto)
        {
            var faq = await _faqRepo.GetByIdAsync(id);
            if (faq == null) return false;
            _mapper.Map(dto, faq);
            faq.UpdatedAt = DateTime.UtcNow;
            await _faqRepo.UpdateAsync(id, faq);
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var faq = await _faqRepo.GetByIdAsync(id);
            if (faq == null) return false;
            await _faqRepo.DeleteAsync(id);
            return true;
        }
    }
}