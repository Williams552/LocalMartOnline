using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.Product;
using LocalMartOnline.Models.DTOs.ProxyShopping;
using LocalMartOnline.Models.DTOs.Seller;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IProxyShopperService
    {
        Task RegisterProxyShopperAsync(ProxyShopperRegistrationRequestDTO dto, string userId);
        Task<ProxyShopperRegistrationResponseDTO?> GetMyRegistrationAsync(string userId);
        Task<List<ProxyShopperRegistrationResponseDTO>> GetAllRegistrationsAsync();
        Task<bool> ApproveRegistrationAsync(ProxyShopperRegistrationApproveDTO dto);

        Task<string> CreateProxyRequestAsync(string buyerId, ProxyRequestDto dto);
        Task<List<ProxyRequest>> GetAvailableRequestsAsync();
        Task<List<ProxyRequest>> GetAvailableRequestsForProxyAsync(string proxyShopperId);
        Task<string?> AcceptRequestAndCreateOrderAsync(string requestId, string proxyShopperId);
        Task<bool> SendProposalAsync(string orderId, ProxyShoppingProposalDTO proposal);
        Task<bool> BuyerApproveAndPayAsync(string orderId, string buyerId);
        Task<bool> StartShoppingAsync(string orderId, string proxyShopperId);
        Task<bool> UploadBoughtItemsAsync(string orderId, string imageUrls, string? note);
        Task<bool> ConfirmDeliveryAsync(string orderId, string buyerId);
        Task<bool> CancelOrderAsync(string orderId, string proxyShopperId, string reason);
        Task<List<ProxyShopperAcceptedRequestDto>> GetMyAcceptedRequestsAsync(string proxyShopperId);
        Task<List<ProxyRequestsResponseDto>> GetMyRequestsAsync(string userId, string userRole);
        Task<List<object>> AdvancedProductSearchAsync(string proxyShopperId, string query, double wPrice, double wReputation, double wSold, double wStock);
        Task<ProxyRequestResponseDto?> GetRequestByIdAsync(string requestId);
        // ADMIN
        Task<List<AdminProxyRequestDto>> GetAllProxyRequestsAsync();
        Task<ProxyRequestsResponseDto?> GetProxyRequestDetailForAdminAsync(string requestId);
        Task<bool> UpdateProxyRequestStatusAsync(string requestId, string status);
        Task<bool> UpdateProxyOrderStatusAsync(string orderId, string status);
        Task<bool> CancelRequestAsync(string requestId, string buyerId);
    }
}
