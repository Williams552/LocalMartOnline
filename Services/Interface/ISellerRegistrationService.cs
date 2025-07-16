using System.Threading.Tasks;
using System.Collections.Generic;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;

namespace LocalMartOnline.Services.Interface
{
    public interface ISellerRegistrationervice
    {
        Task<bool> RegisterAsync(string userId, SellerRegistrationRequestDTO dto);
        Task<SellerRegistrationRequestDTO?> GetMyRegistrationAsync(string userId);
        Task<IEnumerable<SellerRegistrationRequestDTO>> GetAllRegistrationsAsync();
        Task<bool> ApproveAsync(SellerRegistrationApproveDTO dto);
        Task<SellerProfileDTO?> GetSellerProfileAsync(string userId);
    }
}
