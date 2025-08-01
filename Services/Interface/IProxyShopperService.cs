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
        Task<string?> AcceptRequestAndCreateOrderAsync(string requestId, string proxyShopperId);
        Task<bool> SendProposalAsync(string requestId, ProxyShoppingProposalDTO proposal);
        Task<bool> BuyerApproveAndPayAsync(string orderId, string buyerId);
        Task<bool> StartShoppingAsync(string orderId, string proxyShopperId);
        Task<bool> UploadBoughtItemsAsync(string orderId, List<string> imageUrls, string? note);
        Task<bool> ConfirmDeliveryAsync(string orderId, string buyerId);
        Task<bool> CancelOrderAsync(string orderId, string proxyShopperId, string reason);
        Task<List<ProxyRequest>> GetMyAcceptedRequestsAsync(string proxyShopperId);
        Task<List<object>> AdvancedProductSearchAsync(string query, double wPrice, double wReputation, double wSold, double wStock);
        Task<ProxyRequestResponseDto?> GetRequestByIdAsync(string requestId);
        Task<string?> GetOrderIdByRequestIdAsync(string requestId);
        Task<List<BuyerRequestWithProposalDto>> GetMyRequestsWithProposalsAsync(string buyerId);
    }
}
