using LocalMartOnline.Models;

namespace LocalMartOnline.Services
{
    public interface ICartService
    {
        Task<Cart> GetOrCreateCartAsync(long userId);
        Task<IEnumerable<CartItem>> GetCartItemsAsync(long userId);
        Task<bool> AddToCartAsync(long userId, long productId, int quantity);
        Task<bool> UpdateCartItemAsync(long userId, long productId, int newQuantity);
        Task<bool> RemoveFromCartAsync(long userId, long productId);
    }
}
