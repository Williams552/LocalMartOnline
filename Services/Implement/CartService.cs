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
        private readonly IMongoCollection<ProductUnit> _productUnitCollection;
        private readonly IMongoCollection<ProductImage> _productImageCollection;
        private readonly IMongoCollection<Store> _storeCollection;
        private readonly IMongoCollection<User> _userCollection;

        public CartService(IMongoDatabase database)
        {
            _cartCollection = database.GetCollection<Cart>("Carts");
            _cartItemCollection = database.GetCollection<CartItem>("CartItems");
            _productCollection = database.GetCollection<Product>("Products");
            _productUnitCollection = database.GetCollection<ProductUnit>("ProductUnits");
            _productImageCollection = database.GetCollection<ProductImage>("ProductImages");
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
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
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

                var unit = await _productUnitCollection
                    .Find(u => u.Id == product.UnitId)
                    .FirstOrDefaultAsync();

                var unitDisplayName = unit?.DisplayName ?? "kg";

                var image = await _productImageCollection
                    .Find(img => img.ProductId == product.Id)
                    .FirstOrDefaultAsync();

                var imgUrl = image?.ImageUrl ?? string.Empty;

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
                        Images = imgUrl,
                        Unit = unitDisplayName,
                        Description = product.Description,
                        IsAvailable = product.Status == ProductStatus.Active,
                        StockQuantity = product.StockQuantity,
                        MinimumQuantity = product.MinimumQuantity,
                        StoreId = product.StoreId ?? string.Empty,
                        StoreName = store?.Name ?? "Unknown Store",
                        SellerName = seller?.FullName ?? "Unknown Seller"
                    },
                    // Sử dụng giá bargain nếu có, ngược lại dùng giá gốc
                    SubTotal = (item.BargainPrice ?? product.Price) * (decimal)item.Quantity,
                    BargainPrice = item.BargainPrice,
                    BargainId = item.BargainId,
                    IsBargainProduct = !string.IsNullOrEmpty(item.BargainId)
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
                var cartItemFilter = Builders<CartItem>.Filter.And(
                    Builders<CartItem>.Filter.Eq(ci => ci.CartId, cart.Id),
                    Builders<CartItem>.Filter.Eq(ci => ci.ProductId, productId),
                    Builders<CartItem>.Filter.Eq(ci => ci.BargainId, null)
                );
                var existingItem = await _cartItemCollection.Find(cartItemFilter).FirstOrDefaultAsync();

                if (existingItem != null)
                {
                    var newQuantity = existingItem.Quantity + quantity;
                    if (product.StockQuantity > 0 && product.StockQuantity < (decimal)newQuantity)
                        return false;

                    var update = Builders<CartItem>.Update
                        .Set(ci => ci.Quantity, newQuantity)
                        .Set(ci => ci.UpdatedAt, DateTime.Now);

                    await _cartItemCollection.UpdateOneAsync(cartItemFilter, update);
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id!,
                        ProductId = productId,
                        Quantity = quantity,
                        BargainPrice = null,
                        BargainId = null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
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

        public async Task<bool> AddBargainToCartAsync(string userId, string productId, double quantity, decimal bargainPrice, string bargainId)
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

                // Check if bargain item already exists in cart (chỉ từ CÙNG BARGAIN này)
                var cartItemFilter = Builders<CartItem>.Filter.And(
                    Builders<CartItem>.Filter.Eq(ci => ci.CartId, cart.Id),
                    Builders<CartItem>.Filter.Eq(ci => ci.ProductId, productId),
                    Builders<CartItem>.Filter.Eq(ci => ci.BargainId, bargainId) // Phải cùng BargainId
                );
                var existingItem = await _cartItemCollection.Find(cartItemFilter).FirstOrDefaultAsync();

                if (existingItem != null)
                {
                    var newQuantity = existingItem.Quantity + quantity;
                    if (product.StockQuantity > 0 && product.StockQuantity < (decimal)newQuantity)
                        return false;

                    var update = Builders<CartItem>.Update
                        .Set(ci => ci.Quantity, newQuantity)
                        .Set(ci => ci.UpdatedAt, DateTime.Now);

                    await _cartItemCollection.UpdateOneAsync(cartItemFilter, update);
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id!,
                        ProductId = productId,
                        Quantity = quantity,
                        BargainPrice = bargainPrice,
                        BargainId = bargainId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    await _cartItemCollection.InsertOneAsync(cartItem);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding bargain to cart: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCartItemByIdAsync(string userId, string cartItemId, double newQuantity)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentException("User ID cannot be null or empty");
                
                if (string.IsNullOrEmpty(cartItemId))
                    throw new ArgumentException("Cart Item ID cannot be null or empty");
                
                if (newQuantity < 0)
                    return false;

                var cart = await GetOrCreateCartAsync(userId);
                
                // Tìm CartItem theo ID và đảm bảo nó thuộc về user này
                var cartItemFilter = Builders<CartItem>.Filter.And(
                    Builders<CartItem>.Filter.Eq(ci => ci.Id, cartItemId),
                    Builders<CartItem>.Filter.Eq(ci => ci.CartId, cart.Id)
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
                var productFilter = Builders<Product>.Filter.Eq(p => p.Id, cartItem.ProductId);
                var product = await _productCollection.Find(productFilter).FirstOrDefaultAsync();

                if (product == null || product.Status != ProductStatus.Active)
                    return false;

                // Check minimum quantity requirement
                if (newQuantity > 0 && newQuantity < (double)product.MinimumQuantity)
                    return false;

                // Check stock availability
                if (product.StockQuantity > 0 && product.StockQuantity < (decimal)newQuantity)
                    return false;

                // Update cart item quantity
                var update = Builders<CartItem>.Update
                    .Set(ci => ci.Quantity, newQuantity)
                    .Set(ci => ci.UpdatedAt, DateTime.Now);

                await _cartItemCollection.UpdateOneAsync(cartItemFilter, update);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating cart item by ID: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveCartItemByIdAsync(string userId, string cartItemId)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentException("User ID cannot be null or empty");
                
                if (string.IsNullOrEmpty(cartItemId))
                    throw new ArgumentException("Cart Item ID cannot be null or empty");

                var cart = await GetOrCreateCartAsync(userId);
                
                // Tìm và xóa CartItem theo ID, đảm bảo nó thuộc về user này
                var cartItemFilter = Builders<CartItem>.Filter.And(
                    Builders<CartItem>.Filter.Eq(ci => ci.Id, cartItemId),
                    Builders<CartItem>.Filter.Eq(ci => ci.CartId, cart.Id)
                );

                var result = await _cartItemCollection.DeleteOneAsync(cartItemFilter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing cart item by ID: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveBargainFromCartAsync(string userId, string bargainId)
        {
            try
            {
                var cart = await GetOrCreateCartAsync(userId);
                var filter = Builders<CartItem>.Filter.And(
                    Builders<CartItem>.Filter.Eq(ci => ci.CartId, cart.Id),
                    Builders<CartItem>.Filter.Eq(ci => ci.BargainId, bargainId)
                );

                var result = await _cartItemCollection.DeleteOneAsync(filter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing bargain from cart: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            try
            {
                var cart = await GetOrCreateCartAsync(userId);
                var cartItemFilter = Builders<CartItem>.Filter.Eq(ci => ci.CartId, cart.Id);
                var result = await _cartItemCollection.DeleteManyAsync(cartItemFilter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing cart: {ex.Message}");
                return false;
            }
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