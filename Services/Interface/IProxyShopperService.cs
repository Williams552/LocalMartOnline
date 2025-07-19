using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.Product;
using LocalMartOnline.Models.DTOs.ProxyShopping;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IProxyShopperService
    {
        // Registration management
        Task RegisterProxyShopperAsync(ProxyShopperRegistrationRequestDTO dto, string userId);
        Task<ProxyShopperRegistration?> GetMyRegistrationAsync(string userId);
        Task<List<ProxyShopperRegistration>> GetAllRegistrationsAsync();
        Task<bool> ApproveRegistrationAsync(ProxyShopperRegistrationApproveDTO dto);
        
        // Order workflow
        Task<List<ProxyShoppingOrder>> GetAvailableOrdersAsync();
        Task<bool> AcceptOrderAsync(string orderId, string proxyShopperId);
        Task<bool> SendProposalAsync(string orderId, ProxyShoppingProposalDTO proposal);
        Task<bool> ConfirmOrderAsync(string orderId, string proxyShopperId);
        Task<bool> UploadBoughtItemsAsync(string orderId, List<string> imageUrls, string note);
        Task<bool> ConfirmFinalPriceAsync(string orderId, decimal finalPrice);
        Task<bool> ConfirmDeliveryAsync(string orderId);
        Task<bool> ReplaceOrRemoveProductAsync(string orderId, string productId, ProductDto? replacementItem);
        
        // Order management for ProxyShopper
        Task<List<ProxyShoppingOrder>> GetMyOrdersAsync(string proxyShopperId, string? status = null);
        Task<ProxyShoppingOrder?> GetOrderDetailAsync(string orderId, string proxyShopperId);
        Task<List<ProxyShoppingOrder>> GetOrderHistoryAsync(string proxyShopperId, int page = 1, int pageSize = 20);
        Task<bool> CancelOrderAsync(string orderId, string proxyShopperId, string reason);
        
        // Product search
        Task<List<ProductDto>> SmartSearchProductsAsync(string query, int limit = 10);
        
        // Statistics for ProxyShopper
        Task<ProxyShopperStatsDTO> GetMyStatsAsync(string proxyShopperId);
    }
}
