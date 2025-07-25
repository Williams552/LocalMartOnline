using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Cart;

namespace LocalMartOnline.Services
{
    public interface ICartService
    {
        Task<Cart> GetOrCreateCartAsync(string userId);
        Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId);
        Task<IEnumerable<CartItemDto>> GetCartItemsWithDetailsAsync(string userId);
        Task<bool> AddToCartAsync(string userId, string productId, double quantity);
        Task<bool> AddBargainToCartAsync(string userId, string productId, double quantity, decimal bargainPrice, string bargainId);
        Task<bool> UpdateCartItemAsync(string userId, string productId, double newQuantity);
        Task<bool> RemoveFromCartAsync(string userId, string productId);
        Task<bool> RemoveBargainFromCartAsync(string userId, string bargainId);
        Task<bool> ClearCartAsync(string userId);
        Task<CartSummaryDto> GetCartSummaryAsync(string userId);
    }
}