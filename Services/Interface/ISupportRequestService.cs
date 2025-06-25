using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.SupportRequest;

namespace LocalMartOnline.Services.Interface
{
    public interface ISupportRequestService
    {
        Task<IEnumerable<SupportRequestDto>> GetAllSupportRequestsAsync();
        Task<IEnumerable<SupportRequestDto>> GetSupportRequestsByStatusAsync(string status);
        Task<SupportRequestDto?> GetSupportRequestByIdAsync(string id);
        Task<IEnumerable<SupportRequestDto>> GetSupportRequestsByUserIdAsync(string userId);
        Task<string> CreateSupportRequestAsync(string userId, CreateSupportRequestDto createDto);
        Task<bool> RespondToSupportRequestAsync(string id, RespondToSupportRequestDto responseDto);
        Task<bool> UpdateSupportRequestStatusAsync(string id, string status);
        Task<bool> DeleteSupportRequestAsync(string id);
    }
}
