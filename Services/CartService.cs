using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Cart;

namespace LocalMartOnline.Services
{
    public class CartService : ICartService
    {
        private readonly IMongoCollection<Cart> _cartCollection;
        private readonly IMongoCollection<CartItem> _cartItemCollection;
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<Store> _storeCollection;
        private readonly IMongoCollection<User> _userCollection;

        public CartService(IMongoDatabase database)
        {
            _cartCollection = database.GetCollection<Cart>("Carts");
            _cartItemCollection = database.GetCollection<CartItem>("CartItems");
            _productCollection = database.GetCollection<Product>("Products");
            _storeCollection = database.GetCollection<Store>("Stores");
            _userCollection = database.GetCollection<User>("Users");
        }

        public async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty");

            var filter = Builders<Cart>.Filter.Eq(c => c.UserId, userId);
            var cart = await _cartCollection.Find(filter).FirstOrDefaultAsync();

            if (cart == null)
            {
                cart = new Cart 
                { 
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
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

        // Enhanced method to get cart items with product details
        public async Task<IEnumerable<CartItemDto>> GetCartItemsWithDetailsAsync(string userId)
        {
            var cartItems = await GetCartItemsAsync(userId);
            var result = new List<CartItemDto>();

            foreach (var item in cartItems)
            {
                // Get product details
                var product = await _productCollection
                    .Find(p => p.Id == item.ProductId)
                    .FirstOrDefaultAsync();

                if (product == null) continue;

                // Get store and seller details
                Store? store = null;
                User? seller = null;

                if (!string.IsNullOrEmpty(product.StoreId))
                {
                    store = await _storeCollection
                        .Find(s => s.Id == product.StoreId)
                        .FirstOrDefaultAsync();

                    if (store != null && !string.IsNullOrEmpty(store.SellerId))
                    {
                        seller = await _userCollection
                            .Find(u => u.Id == store.SellerId)
                            .FirstOrDefaultAsync();
                    }
                }

                var cartItemDto = new CartItemDto
                {
                    Id = item.Id ?? string.Empty,
                    CartId = item.CartId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt,
                    Product = new ProductInCartDto
                    {
                        Id = product.Id ?? string.Empty,
                        Name = product.Name,
                        Price = product.Price,
                        Images = product.Images?.FirstOrDefault() ?? "", // Get first image or empty string
                        Unit = product.UnitId ?? "kg", // Using UnitId instead of Unit
                        Description = product.Description,
                        IsAvailable = product.Status == ProductStatus.Active,
                        StockQuantity = product.StockQuantity, // Use actual stock quantity
                        MinimumQuantity = product.MinimumQuantity,
                        StoreId = product.StoreId ?? string.Empty,
                        StoreName = store?.Name ?? "Unknown Store",
                        SellerName = seller?.FullName ?? "Unknown Seller"
                    },
                    SubTotal = product.Price * (decimal)item.Quantity
                };

                result.Add(cartItemDto);
            }

            return result;
        }

        public async Task<bool> AddToCartAsync(string userId, string productId, double quantity)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentException("User ID cannot be null or empty");
                
                if (string.IsNullOrEmpty(productId))
                    throw new ArgumentException("Product ID cannot be null or empty");
                
                if (quantity <= 0)
                    return false;

                // Check if product exists and is active
                var productFilter = Builders<Product>.Filter.Eq(p => p.Id, productId);
                var product = await _productCollection.Find(productFilter).FirstOrDefaultAsync();

                if (product == null || product.Status != ProductStatus.Active)
                    return false;

                // Check minimum quantity requirement
                if (quantity < (double)product.MinimumQuantity)
                    return false;

                // Check stock if stock management is enabled
                if (product.StockQuantity > 0 && product.StockQuantity < (decimal)quantity)
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
                    var newQuantity = existingItem.Quantity + quantity;
                    
                    // Check stock again for total quantity
                    if (product.StockQuantity > 0 && product.StockQuantity < (decimal)newQuantity)
                        return false;

                    var update = Builders<CartItem>.Update
                        .Set(ci => ci.Quantity, newQuantity)
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
                        Quantity = quantity,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _cartItemCollection.InsertOneAsync(cartItem);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to cart: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCartItemAsync(string userId, string productId, double newQuantity)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentException("User ID cannot be null or empty");
                
                if (string.IsNullOrEmpty(productId))
                    throw new ArgumentException("Product ID cannot be null or empty");
                
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

                if (product == null || product.Status != ProductStatus.Active)
                    return false;

                // Check minimum quantity requirement (unless removing item)
                if (newQuantity > 0 && newQuantity < (double)product.MinimumQuantity)
                    return false;

                // Check stock availability
                if (product.StockQuantity > 0 && product.StockQuantity < (decimal)newQuantity)
                    return false;

                // Update cart item quantity
                var update = Builders<CartItem>.Update
                    .Set(ci => ci.Quantity, newQuantity)
                    .Set(ci => ci.UpdatedAt, DateTime.UtcNow);

                await _cartItemCollection.UpdateOneAsync(cartItemFilter, update);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating cart item: {ex.Message}");
                return false;
            }
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

        public async Task<bool> ClearCartAsync(string userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var cartItemFilter = Builders<CartItem>.Filter.Eq(ci => ci.CartId, cart.Id);

            var result = await _cartItemCollection.DeleteManyAsync(cartItemFilter);
            return result.DeletedCount > 0;
        }

        // Get cart summary with totals
        public async Task<CartSummaryDto> GetCartSummaryAsync(string userId)
        {
            try
            {
                var cartItems = await GetCartItemsWithDetailsAsync(userId);
                var items = cartItems.ToList();
                
                var totalItems = items.Sum(item => item.Quantity);
                var totalAmount = items.Sum(item => item.SubTotal);

                var estimatedTax = totalAmount * 0.08m; // 8% VAT
                var estimatedTotal = totalAmount + estimatedTax;

                return new CartSummaryDto
                {
                    TotalItems = (int)totalItems,
                    UniqueProducts = items.Count,
                    TotalAmount = totalAmount,
                    EstimatedTax = estimatedTax,
                    EstimatedTotal = estimatedTotal
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cart summary: {ex.Message}");
                return new CartSummaryDto
                {
                    TotalItems = 0,
                    UniqueProducts = 0,
                    TotalAmount = 0m,
                    EstimatedTax = 0m,
                    EstimatedTotal = 0m
                };
            }
        }
    }
}