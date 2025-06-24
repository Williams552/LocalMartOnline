using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Models.DTOs.Product;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Implement
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<ProductImage> _imageRepo;
        private readonly IRepository<Store> _storeRepo;
        private readonly IMapper _mapper;

        public ProductService(
            IRepository<Product> productRepo,
            IRepository<ProductImage> imageRepo,
            IRepository<Store> storeRepo,
            IMapper mapper)
        {
            _productRepo = productRepo;
            _imageRepo = imageRepo;
            _storeRepo = storeRepo;
            _mapper = mapper;
        }

        // UC041: Add Product
        public async Task<ProductDto> AddProductAsync(ProductCreateDto dto)
        {
            var product = _mapper.Map<Product>(dto);
            product.Status = ProductStatus.Active;
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepo.CreateAsync(product);

            // Store images
            foreach (var url in dto.ImageUrls)
            {
                var img = new ProductImage
                {
                    ProductId = product.Id!,
                    ImageUrl = url,
                    CreatedAt = DateTime.UtcNow
                };
                await _imageRepo.CreateAsync(img);
            }

            var productDto = _mapper.Map<ProductDto>(product);
            productDto.ImageUrls = dto.ImageUrls;
            return productDto;
        }

        // UC042: Edit Product
        public async Task<bool> EditProductAsync(string id, ProductUpdateDto dto)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return false;
            _mapper.Map(dto, product);
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepo.UpdateAsync(id, product);

            // Update images: remove old, add new
            var oldImages = await _imageRepo.FindManyAsync(i => i.ProductId == id);
            foreach (var img in oldImages)
                await _imageRepo.DeleteAsync(img.Id!);
            foreach (var url in dto.ImageUrls)
            {
                var img = new ProductImage
                {
                    ProductId = id,
                    ImageUrl = url,
                    CreatedAt = DateTime.UtcNow
                };
                await _imageRepo.CreateAsync(img);
            }
            return true;
        }

        // UC043: Toggle Product
        public async Task<bool> ToggleProductAsync(string id, bool enable)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return false;
            product.Status = enable ? ProductStatus.Active : ProductStatus.Inactive;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepo.UpdateAsync(id, product);
            return true;
        }

        // UC049: View All Product List
        public async Task<PagedResultDto<ProductDto>> GetAllProductsAsync(int page, int pageSize)
        {
            var products = await _productRepo.GetAllAsync();
            var stores = await _storeRepo.GetAllAsync();
            var activeStoreIds = new HashSet<string>(stores.Where(s => s.Status == "Open").Select(s => s.Id));

            var productList = products
                .Where(p => p.Status == ProductStatus.Active && activeStoreIds.Contains(p.StoreId))
                .ToList();

            var total = productList.Count();
            var paged = productList.Skip((page - 1) * pageSize).Take(pageSize);
            var items = await MapProductDtosWithImages(paged);
            return new PagedResultDto<ProductDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // UC050: View Product Details
        public async Task<ProductDto?> GetProductDetailsAsync(string id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null || product.Status != ProductStatus.Active) return null;
            var dto = _mapper.Map<ProductDto>(product);
            var images = await _imageRepo.FindManyAsync(i => i.ProductId == id);
            dto.ImageUrls = images.Select(i => i.ImageUrl).ToList();
            return dto;
        }

        // UC053: Upload Actual Product Photo
        public async Task<bool> UploadActualProductPhotoAsync(ProductActualPhotoUploadDto dto)
        {
            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product == null) return false;
            var image = new ProductImage
            {
                ProductId = dto.ProductId,
                ImageUrl = dto.ImageUrl,
                IsWatermarked = dto.IsWatermarked,
                Timestamp = dto.Timestamp,
                CreatedAt = DateTime.UtcNow
            };
            await _imageRepo.CreateAsync(image);
            return true;
        }

        // UC054: Search Products
        public async Task<PagedResultDto<ProductDto>> SearchProductsAsync(string keyword, string? categoryId, decimal? latitude, decimal? longitude, int page, int pageSize)
        {
            var products = await _productRepo.GetAllAsync();
            var stores = await _storeRepo.GetAllAsync();
            var activeStoreIds = new HashSet<string>(stores.Where(s => s.Status == "Open").Select(s => s.Id));

            var filtered = products.Where(p =>
                p.Status == ProductStatus.Active &&
                activeStoreIds.Contains(p.StoreId) &&
                (string.IsNullOrEmpty(keyword) || p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) || p.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(categoryId) || p.CategoryId == categoryId)
            ).ToList();

            // Nếu có location, lọc theo khoảng cách (giả sử Store có lat/lng)
            if (latitude.HasValue && longitude.HasValue)
            {
                filtered = filtered.Where(p =>
                {
                    var store = stores.FirstOrDefault(s => s.Id == p.StoreId);
                    if (store == null) return false;
                    var dist = GetDistanceKm(latitude.Value, longitude.Value, store.Latitude, store.Longitude);
                    return dist <= 50; // ví dụ: chỉ lấy trong bán kính 50km
                }).ToList();
            }

            var total = filtered.Count();
            var paged = filtered.Skip((page - 1) * pageSize).Take(pageSize);
            var items = await MapProductDtosWithImages(paged);
            return new PagedResultDto<ProductDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }


        // UC055: Filter Products
        public async Task<PagedResultDto<ProductDto>> FilterProductsAsync(ProductFilterDto filter)
        {
            var products = await _productRepo.GetAllAsync();
            var stores = await _storeRepo.GetAllAsync();
            var activeStoreIds = new HashSet<string>(stores.Where(s => s.Status == "Open").Select(s => s.Id));

            var filtered = products.Where(p =>
                p.Status == ProductStatus.Active &&
                activeStoreIds.Contains(p.StoreId) &&
                (string.IsNullOrEmpty(filter.CategoryId) || p.CategoryId == filter.CategoryId) &&
                (!filter.MinPrice.HasValue || p.Price >= filter.MinPrice.Value) &&
                (!filter.MaxPrice.HasValue || p.Price <= filter.MaxPrice.Value) &&
                (string.IsNullOrEmpty(filter.Keyword) || p.Name.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase) || p.Description.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            // Lọc theo vị trí nếu có
            if (filter.Latitude.HasValue && filter.Longitude.HasValue && filter.MaxDistanceKm.HasValue)
            {
                filtered = filtered.Where(p =>
                {
                    var store = stores.FirstOrDefault(s => s.Id == p.StoreId);
                    if (store == null) return false;
                    var dist = GetDistanceKm(filter.Latitude.Value, filter.Longitude.Value, store.Latitude, store.Longitude);
                    return dist <= filter.MaxDistanceKm.Value;
                }).ToList();
            }

            // Sắp xếp
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                if (filter.SortBy == "price")
                {
                    filtered = filter.Ascending == false
                        ? filtered.OrderByDescending(p => p.Price).ToList()
                        : filtered.OrderBy(p => p.Price).ToList();
                }
                // Có thể bổ sung sort theo relevance, distance nếu cần
            }

            var total = filtered.Count();
            var paged = filtered.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);
            var items = await MapProductDtosWithImages(paged);
            return new PagedResultDto<ProductDto>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        // Hàm tính khoảng cách Haversine (km)
        private static double GetDistanceKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            double R = 6371; // Earth radius in km
            double dLat = (double)(lat2 - lat1) * Math.PI / 180.0;
            double dLon = (double)(lon2 - lon1) * Math.PI / 180.0;
            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos((double)lat1 * Math.PI / 180.0) * Math.Cos((double)lat2 * Math.PI / 180.0) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        public async Task<PagedResultDto<ProductDto>> GetProductsByStoreAsync(string storeId, int page, int pageSize)
        {
            var products = await _productRepo.FindManyAsync(p => p.StoreId == storeId && p.Status == ProductStatus.Active);
            var total = products.Count();
            var paged = products.Skip((page - 1) * pageSize).Take(pageSize);
            var items = await MapProductDtosWithImages(paged);
            return new PagedResultDto<ProductDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultDto<ProductDto>> FilterProductsInStoreAsync(ProductFilterDto filter)
        {
            var products = await _productRepo.GetAllAsync();
            var filtered = products.Where(p =>
                p.StoreId == filter.StoreId &&
                (string.IsNullOrEmpty(filter.CategoryId) || p.CategoryId == filter.CategoryId) &&
                (!filter.MinPrice.HasValue || p.Price >= filter.MinPrice.Value) &&
                (!filter.MaxPrice.HasValue || p.Price <= filter.MaxPrice.Value) &&
                (string.IsNullOrEmpty(filter.Name) || p.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase)) &&
                (!filter.MinStock.HasValue || p.StockQuantity >= filter.MinStock.Value) &&
                (!filter.MaxStock.HasValue || p.StockQuantity <= filter.MaxStock.Value) &&
                (string.IsNullOrEmpty(filter.Status) || p.Status.ToString() == filter.Status) &&
                (string.IsNullOrEmpty(filter.Keyword) || p.Name.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase) || p.Description.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            // Sắp xếp nếu cần
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                if (filter.SortBy == "price")
                    filtered = filter.Ascending == false ? filtered.OrderByDescending(p => p.Price).ToList() : filtered.OrderBy(p => p.Price).ToList();
                else if (filter.SortBy == "stock")
                    filtered = filter.Ascending == false ? filtered.OrderByDescending(p => p.StockQuantity).ToList() : filtered.OrderBy(p => p.StockQuantity).ToList();
            }

            var total = filtered.Count();
            var paged = filtered.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);
            var items = await MapProductDtosWithImages(paged);
            return new PagedResultDto<ProductDto>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<PagedResultDto<ProductDto>> SearchProductsInStoreAsync(string storeId, string keyword, int page, int pageSize)
        {
            var products = await _productRepo.FindManyAsync(p =>
                p.StoreId == storeId &&
                (p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) || p.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            );
            var total = products.Count();
            var paged = products.Skip((page - 1) * pageSize).Take(pageSize);
            var items = await MapProductDtosWithImages(paged);
            return new PagedResultDto<ProductDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        //public async Task<ProductDto?> GetProductDetailsInStoreAsync(string storeId, string productId)
        //{
        //    var product = await _productRepo.GetByIdAsync(productId);
        //    if (product == null || product.StoreId != storeId) return null;
        //    var dto = _mapper.Map<ProductDto>(product);
        //    var images = await _imageRepo.FindManyAsync(i => i.ProductId == product.Id);
        //    dto.ImageUrls = images.Select(i => i.ImageUrl).ToList();
        //    return dto;
        //}

        // In ProductService.cs
        public async Task<PagedResultDto<ProductDto>> GetProductsByMarketAsync(string marketId, int page, int pageSize)
        {
            // Get all stores in the market
            var stores = await _storeRepo.FindManyAsync(s => s.MarketId == marketId && s.Status == "Open");
            if (!stores.Any())
                return new PagedResultDto<ProductDto>
                {
                    Items = new List<ProductDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };

            // Get store IDs in this market
            var storeIds = stores.Select(s => s.Id).ToList();

            // Get all active products from these stores
            var products = await _productRepo.FindManyAsync(p =>
                storeIds.Contains(p.StoreId) &&
                p.Status == ProductStatus.Active
            );

            var total = products.Count();
            var paged = products.Skip((page - 1) * pageSize).Take(pageSize);
            var items = await MapProductDtosWithImages(paged);

            return new PagedResultDto<ProductDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultDto<ProductDto>> FilterProductsInMarketAsync(string marketId, ProductFilterDto filter)
        {
            // Get all stores in the market
            var stores = await _storeRepo.FindManyAsync(s => s.MarketId == marketId && s.Status == "Open");
            if (!stores.Any())
                return new PagedResultDto<ProductDto>
                {
                    Items = new List<ProductDto>(),
                    TotalCount = 0,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

            // Get store IDs in this market
            var storeIds = stores.Select(s => s.Id).ToList();

            // Get all products from these stores
            var products = await _productRepo.GetAllAsync();
            var filtered = products.Where(p =>
                storeIds.Contains(p.StoreId) &&
                p.Status == ProductStatus.Active &&
                (string.IsNullOrEmpty(filter.CategoryId) || p.CategoryId == filter.CategoryId) &&
                (!filter.MinPrice.HasValue || p.Price >= filter.MinPrice.Value) &&
                (!filter.MaxPrice.HasValue || p.Price <= filter.MaxPrice.Value) &&
                (string.IsNullOrEmpty(filter.Name) || p.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(filter.Keyword) || p.Name.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase) ||
                 p.Description.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            // Apply sorting if specified
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                if (filter.SortBy == "price")
                    filtered = filter.Ascending == false ? filtered.OrderByDescending(p => p.Price).ToList() : filtered.OrderBy(p => p.Price).ToList();
            }

            var total = filtered.Count();
            var paged = filtered.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);
            var items = await MapProductDtosWithImages(paged);

            return new PagedResultDto<ProductDto>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<PagedResultDto<ProductDto>> SearchProductsInMarketAsync(string marketId, string keyword, int page, int pageSize)
        {
            // Get all stores in the market
            var stores = await _storeRepo.FindManyAsync(s => s.MarketId == marketId && s.Status == "Open");
            if (!stores.Any())
                return new PagedResultDto<ProductDto>
                {
                    Items = new List<ProductDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };

            // Get store IDs in this market
            var storeIds = stores.Select(s => s.Id).ToList();

            // Search products across all stores in the market
            var products = await _productRepo.FindManyAsync(p =>
                storeIds.Contains(p.StoreId) &&
                p.Status == ProductStatus.Active &&
                (p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                 p.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            );

            var total = products.Count();
            var paged = products.Skip((page - 1) * pageSize).Take(pageSize);
            var items = await MapProductDtosWithImages(paged);

            return new PagedResultDto<ProductDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // Helper method
        private async Task<IEnumerable<ProductDto>> MapProductDtosWithImages(IEnumerable<Product> products)
        {
            var result = new List<ProductDto>();
            foreach (var product in products)
            {
                var dto = _mapper.Map<ProductDto>(product);
                var images = await _imageRepo.FindManyAsync(i => i.ProductId == product.Id);
                dto.ImageUrls = images.Select(i => i.ImageUrl).ToList();
                result.Add(dto);
            }
            return result;
        }
    }
}