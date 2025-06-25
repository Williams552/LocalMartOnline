using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using LocalMartOnline.Models;
using LocalMartOnline.Services.Interface;

namespace LocalMartOnline.Services.Implement
{
    public class CartService : ICartService
    {
        private readonly IMongoCollection<Cart> _cartCollection;
        private readonly IMongoCollection<CartItem> _cartItemCollection;
        private readonly IMongoCollection<Product> _productCollection;

        public CartService(IMongoDatabase database)
        {
            _cartCollection = database.GetCollection<Cart>("Carts");
            _cartItemCollection = database.GetCollection<CartItem>("CartItems");
            _productCollection = database.GetCollection<Product>("Products");
        }

        public async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            var filter = Builders<Cart>.Filter.Eq(c => c.UserId, userId);
            var cart = await _cartCollection.Find(filter).FirstOrDefaultAsync();

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _cartCollection.InsertOneAsync(cart);
            }

            return cart;
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var filter = Builders<CartItem>.Filter.Eq(ci => ci.CartId, cart.Id);
            var cartItems = await _cartItemCollection.Find(filter).ToListAsync();
            return cartItems;
        }

        public async Task<bool> AddToCartAsync(string userId, string productId, int quantity)
        {
            // Check if product exists and has enough stock
            var productFilter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var product = await _productCollection.Find(productFilter).FirstOrDefaultAsync();

            if (product == null || product.StockQuantity < quantity)
                return false;

            var cart = await GetOrCreateCartAsync(userId);

            // Check if item already exists in cart
            var cartItemFilter = Builders<CartItem>.Filter.And(
                Builders<CartItem>.Filter.Eq(ci => ci.CartId, cart.Id),
                Builders<CartItem>.Filter.Eq(ci => ci.ProductId, productId)
            );
            var existingItem = await _cartItemCollection.Find(cartItemFilter).FirstOrDefaultAsync();

            if (existingItem != null)
            {
                // Update existing item quantity
                if (product.StockQuantity < existingItem.Quantity + quantity)
                    return false;

                var update = Builders<CartItem>.Update
                    .Set(ci => ci.Quantity, existingItem.Quantity + quantity)
                    .Set(ci => ci.UpdatedAt, DateTime.UtcNow);

                await _cartItemCollection.UpdateOneAsync(cartItemFilter, update);
            }
            else
            {
                // Create new cart item
                var cartItem = new CartItem
                {
                    CartId = cart.Id!,
                    ProductId = productId,
                    Quantity = quantity
                };
                await _cartItemCollection.InsertOneAsync(cartItem);
            }

            return true;
        }

        public async Task<bool> UpdateCartItemAsync(string userId, string productId, int newQuantity)
        {
            if (newQuantity < 0)
                return false;

            var cart = await GetOrCreateCartAsync(userId);
            var cartItemFilter = Builders<CartItem>.Filter.And(
                Builders<CartItem>.Filter.Eq(ci => ci.CartId, cart.Id),
                Builders<CartItem>.Filter.Eq(ci => ci.ProductId, productId)
            );
            var cartItem = await _cartItemCollection.Find(cartItemFilter).FirstOrDefaultAsync();

            if (cartItem == null)
                return false;

            if (newQuantity == 0)
            {
                // Remove item from cart
                await _cartItemCollection.DeleteOneAsync(cartItemFilter);
                return true;
            }

            // Check if product has enough stock
            var productFilter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var product = await _productCollection.Find(productFilter).FirstOrDefaultAsync();

            if (product == null || product.StockQuantity < newQuantity)
                return false;

            // Update cart item quantity
            var update = Builders<CartItem>.Update
                .Set(ci => ci.Quantity, newQuantity)
                .Set(ci => ci.UpdatedAt, DateTime.UtcNow);

            await _cartItemCollection.UpdateOneAsync(cartItemFilter, update);

            return true;
        }

        public async Task<bool> RemoveFromCartAsync(string userId, string productId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var cartItemFilter = Builders<CartItem>.Filter.And(
                Builders<CartItem>.Filter.Eq(ci => ci.CartId, cart.Id),
                Builders<CartItem>.Filter.Eq(ci => ci.ProductId, productId)
            );

            var result = await _cartItemCollection.DeleteOneAsync(cartItemFilter);
            return result.DeletedCount > 0;
        }
    }
}