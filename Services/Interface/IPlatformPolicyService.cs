using LocalMartOnline.Models.DTOs.PlatformPolicy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IPlatformPolicyService
    {
        Task<IEnumerable<PlatformPolicyDto>> GetAllAsync(PlatformPolicyFilterDto? filter = null);
        Task<PlatformPolicyDto?> GetByIdAsync(string id);
        Task<PlatformPolicyDto> CreateAsync(PlatformPolicyCreateDto dto);
        Task<bool> UpdateAsync(string id, PlatformPolicyUpdateDto dto);
        Task<bool> ToggleAsync(string id);
    }
}