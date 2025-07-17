using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.Product;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
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
        Task<bool> ReplaceOrRemoveProductAsync(string orderId, string productId, ProductDto? replacementItem);
        Task<List<ProductDto>> SmartSearchProductsAsync(string query, int limit = 10);
    }
}
