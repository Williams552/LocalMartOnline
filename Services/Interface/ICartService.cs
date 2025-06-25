using LocalMartOnline.Models;

namespace LocalMartOnline.Services.Interface
{
    public interface ICartService
    {
        Task<Cart> GetOrCreateCartAsync(string userId);
        Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId);
        Task<bool> AddToCartAsync(string userId, string productId, int quantity);
        Task<bool> UpdateCartItemAsync(string userId, string productId, int newQuantity);
        Task<bool> RemoveFromCartAsync(string userId, string productId);
    }
}