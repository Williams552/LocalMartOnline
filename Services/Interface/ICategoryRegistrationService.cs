using LocalMartOnline.Models.DTOs.CategoryRegistration;
using LocalMartOnline.Models.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface ICategoryRegistrationService
    {
        Task<PagedResultDto<CategoryRegistrationDto>> GetAllPagedAsync(int page, int pageSize);
        Task<bool> ApproveAsync(string id);
        Task<bool> RejectAsync(string id, string rejectionReason);
        Task<CategoryRegistrationDto> RegisterAsync(CategoryRegistrationCreateDto dto);
    }
}
