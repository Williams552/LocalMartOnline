using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Favorite;
using LocalMartOnline.Repositories;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocalMartOnline.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IRepository<Favorite> _favoriteRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<ProductImage> _imageRepo;
        private readonly IRepository<Store> _storeRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IMapper _mapper;

        public FavoriteService(
            IRepository<Favorite> favoriteRepo,
            IRepository<Product> productRepo,
            IRepository<ProductImage> imageRepo,
            IRepository<Store> storeRepo,
            IRepository<Category> categoryRepo,
            IMapper mapper)
        {
            _favoriteRepo = favoriteRepo;
            _productRepo = productRepo;
            _imageRepo = imageRepo;
            _storeRepo = storeRepo;
            _categoryRepo = categoryRepo;
            _mapper = mapper;
        }

        public async Task<FavoriteActionResponseDto> AddToFavoriteAsync(string userId, string productId)
        {
            try
            {
                // Check if already in favorites
                var existingFavorite = await _favoriteRepo
                    .FindOneAsync(f => f.UserId == userId && f.ProductId == productId);

                if (existingFavorite != null)
                {
                    return new FavoriteActionResponseDto
                    {
                        Success = false,
                        Message = "Sản phẩm đã có trong danh sách yêu thích"
                    };
                }

                // Check if product exists and is active
                var product = await _productRepo.GetByIdAsync(productId);
                if (product == null || product.Status != ProductStatus.Active)
                {
                    return new FavoriteActionResponseDto
                    {
                        Success = false,
                        Message = "Sản phẩm không tồn tại hoặc không khả dụng"
                    };
                }

                var favorite = new Favorite
                {
                    UserId = userId,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                };

                await _favoriteRepo.CreateAsync(favorite);

                return new FavoriteActionResponseDto
                {
                    Success = true,
                    Message = "Đã thêm sản phẩm vào danh sách yêu thích"
                };
            }
            catch (Exception ex)
            {
                return new FavoriteActionResponseDto
                {
                    Success = false,
                    Message = $"Lỗi khi thêm vào yêu thích: {ex.Message}"
                };
            }
        }

        public async Task<GetFavoriteProductsResponseDto> GetUserFavoriteProductsAsync(string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                // Get user's favorites
                var favorites = await _favoriteRepo.FindManyAsync(f => f.UserId == userId);
                var productIds = favorites.Select(f => f.ProductId).ToList();

                if (!productIds.Any())
                {
                    return new GetFavoriteProductsResponseDto
                    {
                        FavoriteProducts = new List<FavoriteProductDto>(),
                        TotalCount = 0,
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalPages = 0
                    };
                }

                // Get products that are still active
                var products = await _productRepo.GetAllAsync();
                var activeProducts = products.Where(p =>
                    productIds.Contains(p.Id) &&
                    p.Status == ProductStatus.Active).ToList();

                // Get additional data
                var stores = await _storeRepo.GetAllAsync();
                var categories = await _categoryRepo.GetAllAsync();
                var allImages = await _imageRepo.GetAllAsync();

                var favoriteProducts = new List<FavoriteProductDto>();

                foreach (var product in activeProducts)
                {
                    var favorite = favorites.First(f => f.ProductId == product.Id);
                    var store = stores.FirstOrDefault(s => s.Id == product.StoreId);
                    var category = categories.FirstOrDefault(c => c.Id == product.CategoryId);
                    var images = allImages.Where(i => i.ProductId == product.Id).ToList();

                    var favoriteProduct = new FavoriteProductDto
                    {
                        FavoriteId = favorite.Id!,
                        ProductId = product.Id!,
                        ProductName = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        Status = product.Status.ToString(),
                        CategoryName = category?.Name ?? "Không xác định",
                        StoreName = store?.Name ?? "Không xác định",
                        StoreId = product.StoreId,
                        ImageUrl = images.FirstOrDefault()?.ImageUrl,
                        AddedToFavoriteAt = favorite.CreatedAt
                    };

                    favoriteProducts.Add(favoriteProduct);
                }

                // Sort by most recently added
                favoriteProducts = favoriteProducts
                    .OrderByDescending(f => f.AddedToFavoriteAt)
                    .ToList();

                // Apply pagination
                var total = favoriteProducts.Count;
                var paged = favoriteProducts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new GetFavoriteProductsResponseDto
                {
                    FavoriteProducts = paged,
                    TotalCount = total,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                };
            }
            catch (Exception ex)
            {
                return new GetFavoriteProductsResponseDto
                {
                    FavoriteProducts = new List<FavoriteProductDto>(),
                    TotalCount = 0,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = 0
                };
            }
        }

        public async Task<FavoriteActionResponseDto> RemoveFromFavoriteAsync(string userId, string productId)
        {
            try
            {
                var favorite = await _favoriteRepo.FindOneAsync(f => f.UserId == userId && f.ProductId == productId);
                if (favorite == null)
                {
                    return new FavoriteActionResponseDto
                    {
                        Success = false,
                        Message = "Sản phẩm không có trong danh sách yêu thích"
                    };
                }

                await _favoriteRepo.DeleteAsync(favorite.Id!);

                return new FavoriteActionResponseDto
                {
                    Success = true,
                    Message = "Đã xóa sản phẩm khỏi danh sách yêu thích"
                };
            }
            catch (Exception ex)
            {
                return new FavoriteActionResponseDto
                {
                    Success = false,
                    Message = $"Lỗi khi xóa khỏi yêu thích: {ex.Message}"
                };
            }
        }

        public async Task<bool> IsProductInFavoriteAsync(string userId, string productId)
        {
            var favorite = await _favoriteRepo.FindOneAsync(f => f.UserId == userId && f.ProductId == productId);
            return favorite != null;
        }
    }
}