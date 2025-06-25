using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.FastBargain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IFastBargainService
    {
        Task<FastBargainResponseDTO> StartBargainAsync(FastBargainCreateRequestDTO request);
        Task<FastBargainResponseDTO> ProposeAsync(FastBargainProposalDTO proposal);
        Task<FastBargainResponseDTO> TakeActionAsync(FastBargainActionRequestDTO request);
        Task<FastBargainResponseDTO?> GetByIdAsync(string bargainId);
        Task<List<FastBargainResponseDTO>> GetByUserIdAsync(string userId);
    }
}
