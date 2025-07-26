using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Models.DTOs.Product;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using MongoDB.Bson;
using MongoDB.Driver;
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
        private readonly IRepository<ProductUnit> _unitRepo;
        private readonly IRepository<Market> _marketRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;
        private readonly IMongoCollection<BsonDocument> _productCollection;

        public ProductService(
            IRepository<Product> productRepo,
            IRepository<ProductImage> imageRepo,
            IRepository<Store> storeRepo,
            IRepository<ProductUnit> unitRepo,
            IRepository<Market> marketRepo,
            IRepository<User> userRepo,
            IMapper mapper,
            IMongoDatabase database)
        {
            _productRepo = productRepo;
            _imageRepo = imageRepo;
            _storeRepo = storeRepo;
            _unitRepo = unitRepo;
            _marketRepo = marketRepo;
            _userRepo = userRepo;
            _mapper = mapper;
            _productCollection = database.GetCollection<BsonDocument>("products");
        }

        // UC041: Add Product
        public async Task<ProductDto> AddProductAsync(ProductCreateDto dto)
        {
            var product = _mapper.Map<Product>(dto);
            product.Status = ProductStatus.Active;
            product.CreatedAt = DateTime.Now;
            product.UpdatedAt = DateTime.Now;
            await _productRepo.CreateAsync(product);

            // Store images
            foreach (var url in dto.ImageUrls)
            {
                var img = new ProductImage
                {
                    ProductId = product.Id!,
                    ImageUrl = url,
                    CreatedAt = DateTime.Now
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
            if (product == null || product.Status == ProductStatus.Inactive) return false;

            _mapper.Map(dto, product);
            product.UpdatedAt = DateTime.Now;
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
                    CreatedAt = DateTime.Now
                };
                await _imageRepo.CreateAsync(img);
            }
            return true;
        }

        // UC043: Toggle Product Status (Active/OutOfStock/Inactive)
        public async Task<bool> ToggleProductStatusAsync(string id, ProductStatus newStatus)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return false;

            // Update status to the specified new status
            product.Status = newStatus;
            product.UpdatedAt = DateTime.Now;
            await _productRepo.UpdateAsync(id, product);
            return true;
        }

        // Delete Product (Soft delete - set to Inactive)
        public async Task<bool> DeleteProductAsync(string id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return false;

            product.Status = ProductStatus.Inactive;
            product.UpdatedAt = DateTime.Now;
            await _productRepo.UpdateAsync(id, product);
            return true;
        }

        // UC049: View All Product List (FOR BUYERS - ACTIVE AND OUT OF STOCK)
        public async Task<PagedResultDto<ProductDto>> GetAllProductsAsync(int page, int pageSize)
        {
            var products = await _productRepo.GetAllAsync();
            var stores = await _storeRepo.GetAllAsync();
            var units = await _unitRepo.GetAllAsync();
            var users = await _userRepo.GetAllAsync();
            var markets = await _marketRepo.GetAllAsync();
            
            var activeStoreIds = new HashSet<string>(stores.Where(s => s.Status == "Open").Select(s => s.Id!).Where(id => id != null));

            var productList = products
                .Where(p => (p.Status == ProductStatus.Active || p.Status == ProductStatus.OutOfStock) && activeStoreIds.Contains(p.StoreId))
                .ToList();

            var total = productList.Count();
            var paged = productList.Skip((page - 1) * pageSize).Take(pageSize);
            var items = await MapProductDtosWithImages(paged);

            // Set StoreName, UnitName, Seller và MarketName cho từng ProductDto
            var storeDict = stores.ToDictionary(s => s.Id!, s => s);
            var unitDict = units.ToDictionary(u => u.Id!, u => u.Name);
            var userDict = users.ToDictionary(u => u.Id!, u => u);
            var marketDict = markets.ToDictionary(m => m.Id!, m => m.Name);
            
            foreach (var dto in items)
            {
                // Get store info first
                var store = (!string.IsNullOrEmpty(dto.StoreId) && storeDict.TryGetValue(dto.StoreId, out var storeInfo)) ? storeInfo : null;
                
                dto.StoreName = store?.Name ?? string.Empty;
                dto.UnitName = (!string.IsNullOrEmpty(dto.UnitId) && unitDict.TryGetValue(dto.UnitId, out var unitName)) ? unitName : string.Empty;
                
                // Set MarketName
                if (store != null && !string.IsNullOrEmpty(store.MarketId) && marketDict.TryGetValue(store.MarketId, out var marketName))
                {
                    dto.MarketName = marketName;
                }
                
                // Set Seller information
                if (store != null && !string.IsNullOrEmpty(store.SellerId) && userDict.TryGetValue(store.SellerId, out var seller))
                {
                    dto.Seller = new SellerDto
                    {
                        Name = seller.FullName,
                        Rating = 0, // Default rating for now
                        Market = dto.MarketName
                    };
                }
                
                // Ensure Status is set
                var product = products.FirstOrDefault(p => p.Id == dto.Id);
                if (product != null)
                    dto.Status = product.Status;
            }

            return new PagedResultDto<ProductDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // UC050: View Product Details (FOR BUYERS - ONLY ACTIVE)
        public async Task<ProductDto?> GetProductDetailsAsync(string id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            var dto = _mapper.Map<ProductDto>(product);
            var images = await _imageRepo.FindManyAsync(i => i.ProductId == id);
            dto.ImageUrls = images.Select(i => i.ImageUrl).ToList();

            // Enrich StoreName, UnitName, and MarketName
            string storeName = string.Empty;
            string unitName = string.Empty;
            string marketName = string.Empty;
            
            if (!string.IsNullOrEmpty(dto.StoreId))
            {
                var store = await _storeRepo.GetByIdAsync(dto.StoreId);
                if (store != null)
                {
                    storeName = store.Name;
                    // Get market name through store's MarketId
                    if (!string.IsNullOrEmpty(store.MarketId))
                    {
                        var market = await _marketRepo.GetByIdAsync(store.MarketId);
                        if (market != null)
                            marketName = market.Name;
                    }
                }
            }
            if (!string.IsNullOrEmpty(dto.UnitId))
            {
                var unit = await _unitRepo.GetByIdAsync(dto.UnitId);
                if (unit != null)
                    unitName = unit.Name;
            }
            dto.StoreName = storeName;
            dto.UnitName = unitName;
            dto.MarketName = marketName;
            // Ensure Status is set
            dto.Status = product.Status;
            return dto;
        }

        // UC053: Upload Actual Product Photo
        public async Task<bool> UploadActualProductPhotoAsync(ProductActualPhotoUploadDto dto)
        {
            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product == null || product.Status == ProductStatus.Inactive) return false;

            var image = new ProductImage
            {
                ProductId = dto.ProductId,
                ImageUrl = dto.ImageUrl,
                IsWatermarked = dto.IsWatermarked,
                Timestamp = dto.Timestamp,
                CreatedAt = DateTime.Now
            };
            await _imageRepo.CreateAsync(image);
            return true;
        }

        // UC054: Search Products (FOR BUYERS - ONLY ACTIVE)
        public async Task<PagedResultDto<ProductDto>> SearchProductsAsync(string keyword, string? categoryId, decimal? latitude, decimal? longitude, int page, int pageSize)
        {
            var products = await _productRepo.GetAllAsync();
            var stores = await _storeRepo.GetAllAsync();
            var activeStoreIds = new HashSet<string>(stores.Where(s => s.Status == "Open").Select(s => s.Id!).Where(id => id != null));

            var filtered = products.Where(p =>
                p.Status == ProductStatus.Active &&
                activeStoreIds.Contains(p.StoreId) &&
                (string.IsNullOrEmpty(keyword) || p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) || p.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(categoryId) || p.CategoryId == categoryId)
            ).ToList();

            // Lọc theo location nếu có
            if (latitude.HasValue && longitude.HasValue)
            {
                filtered = filtered.Where(p =>
                {
                    var store = stores.FirstOrDefault(s => s.Id == p.StoreId);
                    if (store == null) return false;
                    var dist = GetDistanceKm(latitude.Value, longitude.Value, store.Latitude, store.Longitude);
                    return dist <= 50;
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

        // UC055: Filter Products (FOR BUYERS - ONLY ACTIVE)
        public async Task<PagedResultDto<ProductDto>> FilterProductsAsync(ProductFilterDto filter)
        {
            var products = await _productRepo.GetAllAsync();
            var stores = await _storeRepo.GetAllAsync();
            
            Console.WriteLine($"FilterProductsAsync called with MarketId: {filter.MarketId}, StoreId: {filter.StoreId}, CategoryId: {filter.CategoryId}, Keyword: {filter.Keyword}");
            Console.WriteLine($"Total products: {products.Count()}, Total stores: {stores.Count()}");
            
            // Start with all open stores
            var validStoreIds = new HashSet<string>(stores.Where(s => s.Status == "Open").Select(s => s.Id!).Where(id => id != null));
            Console.WriteLine($"Open stores count: {validStoreIds.Count}");

            // Filter by market if specified
            if (!string.IsNullOrEmpty(filter.MarketId))
            {
                validStoreIds = new HashSet<string>(stores
                    .Where(s => s.MarketId == filter.MarketId && s.Status == "Open")
                    .Select(s => s.Id!)
                    .Where(id => id != null));
                Console.WriteLine($"Stores in market {filter.MarketId}: {validStoreIds.Count}");
            }

            // Further filter by specific store if specified
            if (!string.IsNullOrEmpty(filter.StoreId))
            {
                validStoreIds = validStoreIds.Where(id => id == filter.StoreId).ToHashSet();
                Console.WriteLine($"After store filter: {validStoreIds.Count}");
            }

            var filtered = products.Where(p =>
                p.Status == ProductStatus.Active &&
                validStoreIds.Contains(p.StoreId) &&
                (string.IsNullOrEmpty(filter.CategoryId) || p.CategoryId == filter.CategoryId) &&
                (!filter.MinPrice.HasValue || p.Price >= filter.MinPrice.Value) &&
                (!filter.MaxPrice.HasValue || p.Price <= filter.MaxPrice.Value) &&
                (string.IsNullOrEmpty(filter.Keyword) || p.Name.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase) || p.Description.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase))
            ).ToList();
            
            Console.WriteLine($"Filtered products count: {filtered.Count}");

            // Lọc theo vị trí nếu có
            if (filter.Latitude.HasValue && filter.Longitude.HasValue && filter.MaxDistanceKm != null)
            {
                filtered = filtered.Where(p =>
                {
                    var store = stores.FirstOrDefault(s => s.Id == p.StoreId);
                    if (store == null) return false;
                    var dist = GetDistanceKm(filter.Latitude.Value, filter.Longitude.Value, store.Latitude, store.Longitude);
                    return dist <= (double)filter.MaxDistanceKm;
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

        // FOR BUYERS - Get products by store (ONLY ACTIVE)
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

        // FOR BUYERS - Filter products in store (ONLY ACTIVE)
        public async Task<PagedResultDto<ProductDto>> FilterProductsInStoreAsync(ProductFilterDto filter)
        {
            var products = await _productRepo.GetAllAsync();
            var filtered = products.Where(p =>
                p.StoreId == filter.StoreId &&
                p.Status == ProductStatus.Active && // Chỉ lấy Active products
                (string.IsNullOrEmpty(filter.CategoryId) || p.CategoryId == filter.CategoryId) &&
                (!filter.MinPrice.HasValue || p.Price >= filter.MinPrice.Value) &&
                (!filter.MaxPrice.HasValue || p.Price <= filter.MaxPrice.Value) &&
                (string.IsNullOrEmpty(filter.Name) || p.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(filter.Keyword) || p.Name.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase) || p.Description.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            // Sắp xếp
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

        // FOR BUYERS - Search products in store (ONLY ACTIVE)
        public async Task<PagedResultDto<ProductDto>> SearchProductsInStoreAsync(string storeId, string keyword, int page, int pageSize)
        {
            // Get all products from the store first, then filter in memory
            var products = await _productRepo.FindManyAsync(p =>
                p.StoreId == storeId &&
                p.Status == ProductStatus.Active
            );

            // Filter by keyword in memory (case-insensitive)
            var filtered = products.Where(p =>
                p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            ).ToList();

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

        // FOR BUYERS - Get products by market (ONLY ACTIVE)
        public async Task<PagedResultDto<ProductDto>> GetProductsByMarketAsync(string marketId, int page, int pageSize)
        {
            var stores = await _storeRepo.FindManyAsync(s => s.MarketId == marketId && s.Status == "Open");
            if (!stores.Any())
                return new PagedResultDto<ProductDto>
                {
                    Items = new List<ProductDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };

            var storeIds = stores.Select(s => s.Id).ToList();
            var products = await _productRepo.FindManyAsync(p =>
                storeIds.Contains(p.StoreId) &&
                p.Status == ProductStatus.Active // Chỉ lấy Active products
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

        // FOR BUYERS - Filter products in market (ONLY ACTIVE)
        public async Task<PagedResultDto<ProductDto>> FilterProductsInMarketAsync(string marketId, ProductFilterDto filter)
        {
            var stores = await _storeRepo.FindManyAsync(s => s.MarketId == marketId && s.Status == "Open");
            if (!stores.Any())
                return new PagedResultDto<ProductDto>
                {
                    Items = new List<ProductDto>(),
                    TotalCount = 0,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

            var storeIds = stores.Select(s => s.Id).ToList();
            var products = await _productRepo.GetAllAsync();
            var filtered = products.Where(p =>
                storeIds.Contains(p.StoreId) &&
                p.Status == ProductStatus.Active && // Chỉ lấy Active products
                (string.IsNullOrEmpty(filter.CategoryId) || p.CategoryId == filter.CategoryId) &&
                (!filter.MinPrice.HasValue || p.Price >= filter.MinPrice.Value) &&
                (!filter.MaxPrice.HasValue || p.Price <= filter.MaxPrice.Value) &&
                (string.IsNullOrEmpty(filter.Name) || p.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(filter.Keyword) || p.Name.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase) ||
                 p.Description.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase))
            ).ToList();

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
            var stores = await _storeRepo.FindManyAsync(s => s.MarketId == marketId && s.Status == "Open");
            if (!stores.Any())
                return new PagedResultDto<ProductDto>
                {
                    Items = new List<ProductDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };

            var storeIds = stores.Select(s => s.Id).ToList();

            // Get all products from the stores first, then filter in memory
            var products = await _productRepo.FindManyAsync(p =>
                storeIds.Contains(p.StoreId) &&
                p.Status == ProductStatus.Active
            );

            // Filter by keyword in memory (case-insensitive)
            var filtered = products.Where(p =>
                p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            ).ToList();

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

        // ============ SELLER METHODS - INCLUDE ACTIVE & OUT OF STOCK (NO INACTIVE) ============

        // FOR SELLERS - Get all products (Active + OutOfStock)
        public async Task<PagedResultDto<ProductDto>> GetAllProductsForSellerAsync(string storeId, int page, int pageSize)
        {
            var products = await _productRepo.FindManyAsync(p =>
                p.StoreId == storeId &&
                (p.Status == ProductStatus.Active || p.Status == ProductStatus.OutOfStock)
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

        // FOR SELLERS - Search products (Active + OutOfStock)
        public async Task<PagedResultDto<ProductDto>> SearchProductsForSellerAsync(string storeId, string keyword, int page, int pageSize)
        {
            // Get all products from the store first, then filter in memory
            var products = await _productRepo.FindManyAsync(p =>
                p.StoreId == storeId &&
                (p.Status == ProductStatus.Active || p.Status == ProductStatus.OutOfStock)
            );

            // Filter by keyword in memory (case-insensitive)
            var filtered = products.Where(p =>
                p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            ).ToList();

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

        // FOR SELLERS - Filter products (Active + OutOfStock)
        public async Task<PagedResultDto<ProductDto>> FilterProductsForSellerAsync(ProductFilterDto filter)
        {
            var products = await _productRepo.GetAllAsync();

            var filtered = products.Where(p =>
                p.StoreId == filter.StoreId &&
                (p.Status == ProductStatus.Active || p.Status == ProductStatus.OutOfStock) &&
                (string.IsNullOrEmpty(filter.CategoryId) || p.CategoryId == filter.CategoryId) &&
                (!filter.MinPrice.HasValue || p.Price >= filter.MinPrice.Value) &&
                (!filter.MaxPrice.HasValue || p.Price <= filter.MaxPrice.Value) &&
                (string.IsNullOrEmpty(filter.Name) || p.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(filter.Status) || p.Status.ToString() == filter.Status) &&
                (string.IsNullOrEmpty(filter.Keyword) ||
                 p.Name.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase) ||
                 p.Description.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            // Apply sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy.ToLower())
                {
                    case "price":
                        filtered = filter.Ascending == false
                            ? filtered.OrderByDescending(p => p.Price).ToList()
                            : filtered.OrderBy(p => p.Price).ToList();
                        break;
                    case "name":
                        filtered = filter.Ascending == false
                            ? filtered.OrderByDescending(p => p.Name).ToList()
                            : filtered.OrderBy(p => p.Name).ToList();
                        break;
                    case "status":
                        filtered = filter.Ascending == false
                            ? filtered.OrderByDescending(p => p.Status).ToList()
                            : filtered.OrderBy(p => p.Status).ToList();
                        break;
                    case "created":
                        filtered = filter.Ascending == false
                            ? filtered.OrderByDescending(p => p.CreatedAt).ToList()
                            : filtered.OrderBy(p => p.CreatedAt).ToList();
                        break;
                    case "updated":
                        filtered = filter.Ascending == false
                            ? filtered.OrderByDescending(p => p.UpdatedAt).ToList()
                            : filtered.OrderBy(p => p.UpdatedAt).ToList();
                        break;
                }
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

        // Helper methods
        private static double GetDistanceKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            double R = 6371;
            double dLat = (double)(lat2 - lat1) * Math.PI / 180.0;
            double dLon = (double)(lon2 - lon1) * Math.PI / 180.0;
            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos((double)lat1 * Math.PI / 180.0) * Math.Cos((double)lat2 * Math.PI / 180.0) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

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

        public async Task<SearchProductResultDto> SearchProductsAsync(SearchProductRequestDto request)
        {
            var products = await _productRepo.GetAllAsync();

            // Chỉ lấy products có status Active cho buyers
            products = products.Where(p => p.Status == ProductStatus.Active).ToList();

            if (!string.IsNullOrEmpty(request.CategoryId))
                products = products.Where(p => p.CategoryId == request.CategoryId).ToList();

            if (!string.IsNullOrEmpty(request.Search))
                products = products.Where(p => p.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) || (p.Description != null && p.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase))).ToList();

            if (!string.IsNullOrEmpty(request.SortPrice))
            {
                if (request.SortPrice.ToLower() == "asc")
                    products = products.OrderBy(p => p.Price).ToList();
                else if (request.SortPrice.ToLower() == "desc")
                    products = products.OrderByDescending(p => p.Price).ToList();
            }

            int total = products.Count();
            int page = request.Page > 0 ? request.Page : 1;
            int pageSize = request.PageSize > 0 ? request.PageSize : 20;
            var paged = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var productDtos = paged.Select(p => new SearchProductResponseDto
            {
                ProductId = p.Id ?? string.Empty,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Status = p.Status.ToString(),
                CategoryName = string.Empty,
                StoreName = string.Empty,
                StoreId = p.StoreId ?? string.Empty,
                ImageUrl = null
            }).ToList();

            return new SearchProductResultDto
            {
                Products = productDtos,
                TotalCount = total,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)total / pageSize)
            };
        }
    }
}