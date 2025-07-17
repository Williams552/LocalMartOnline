using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services
{
    public interface IProxyShopperService
    {
        Task RegisterProxyShopperAsync(ProxyShopperRegistrationRequestDTO dto, string userId);
        Task<ProxyShopperRegistration?> GetMyRegistrationAsync(string userId);
        Task<List<ProxyShopperRegistration>> GetAllRegistrationsAsync();
        Task<bool> ApproveRegistrationAsync(ProxyShopperRegistrationApproveDTO dto);
        // ...existing methods for order workflow...
        Task<List<ProxyShoppingOrder>> GetAvailableOrdersAsync();
        Task<bool> AcceptOrderAsync(string orderId, string proxyShopperId);
        Task<bool> SendProposalAsync(string orderId, ProxyShoppingProposalDTO proposal);
        Task<bool> ConfirmOrderAsync(string orderId, string proxyShopperId);
        Task<bool> UploadBoughtItemsAsync(string orderId, List<string> imageUrls, string note);
        Task<bool> ConfirmFinalPriceAsync(string orderId, decimal finalPrice);
        Task<bool> ConfirmDeliveryAsync(string orderId);
    }
}
