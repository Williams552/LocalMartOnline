using LocalMartOnline.Models;
using LocalMartOnline.Repositories;

namespace LocalMartOnline.Services
{
    public class CartService : ICartService
    {
        private readonly IGenericRepository<Cart> _cartRepository;
        private readonly IGenericRepository<CartItem> _cartItemRepository;
        private readonly IGenericRepository<Product> _productRepository;

        public CartService(
            IGenericRepository<Cart> cartRepository,
            IGenericRepository<CartItem> cartItemRepository,
            IGenericRepository<Product> productRepository)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
        }

        public async Task<Cart> GetOrCreateCartAsync(long userId)
        {
            var carts = await _cartRepository.GetAllAsync();
            var cart = carts.FirstOrDefault(c => c.UserId == userId);
            
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _cartRepository.CreateAsync(cart);
            }
            
            return cart;
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(long userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var cartItems = await _cartItemRepository.GetAllAsync();
            return cartItems.Where(ci => ci.CartId == cart.CartId);
        }

        public async Task<bool> AddToCartAsync(long userId, long productId, int quantity)
        {
            var product = (await _productRepository.GetAllAsync())
                .FirstOrDefault(p => p.ProductId == productId);
            
            if (product == null || product.StockQuantity < quantity)
                return false;

            var cart = await GetOrCreateCartAsync(userId);
            var cartItems = await _cartItemRepository.GetAllAsync();
            var existingItem = cartItems.FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductId == productId);

            if (existingItem != null)
            {
                if (product.StockQuantity < existingItem.Quantity + quantity)
                    return false;
                
                existingItem.Quantity += quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                await _cartItemRepository.UpdateAsync(existingItem.Id!, existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity
                };
                await _cartItemRepository.CreateAsync(cartItem);
            }

            return true;
        }

        public async Task<bool> UpdateCartItemAsync(long userId, long productId, int newQuantity)
        {
            if (newQuantity < 0)
                return false;

            var cart = await GetOrCreateCartAsync(userId);
            var cartItems = await _cartItemRepository.GetAllAsync();
            var cartItem = cartItems.FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductId == productId);
            
            if (cartItem == null)
                return false;

            if (newQuantity == 0)
            {
                await _cartItemRepository.DeleteAsync(cartItem.Id!);
                return true;
            }

            var product = (await _productRepository.GetAllAsync())
                .FirstOrDefault(p => p.ProductId == productId);
            
            if (product == null || product.StockQuantity < newQuantity)
                return false;

            cartItem.Quantity = newQuantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            await _cartItemRepository.UpdateAsync(cartItem.Id!, cartItem);
            
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(long userId, long productId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var cartItems = await _cartItemRepository.GetAllAsync();
            var cartItem = cartItems.FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductId == productId);
            
            if (cartItem == null)
                return false;

            await _cartItemRepository.DeleteAsync(cartItem.Id!);
            return true;
        }
    }
}
