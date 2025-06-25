using LocalMartOnline.Models.DTOs.PlatformPolicy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IPlatformPolicyService
    {
        Task<IEnumerable<PlatformPolicyDto>> GetAllAsync();
        Task<PlatformPolicyDto?> GetByIdAsync(string id);
        Task<bool> UpdateAsync(string id, PlatformPolicyUpdateDto dto);
    }
}