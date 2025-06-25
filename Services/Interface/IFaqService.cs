using LocalMartOnline.Models.DTOs.Faq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IFaqService
    {
        Task<IEnumerable<FaqDto>> GetAllAsync();
        Task<FaqDto?> GetByIdAsync(string id);
        Task<FaqDto> AddAsync(FaqCreateDto dto);
        Task<bool> UpdateAsync(string id, FaqUpdateDto dto);
        Task<bool> DeleteAsync(string id);
    }
}