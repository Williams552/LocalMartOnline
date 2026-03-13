using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Models.DTOs.Product;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
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
        private readonly IMarketService _marketService;
        private readonly IMapper _mapper;
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<ProductImage> _productImageCollection;
        private readonly IMongoCollection<Store> _storeCollection;
        private readonly IMongoCollection<ProductUnit> _unitCollection;
        private readonly IMongoCollection<Market> _marketCollection;
        private readonly IMongoCollection<User> _userCollection;

        public ProductService(
            IRepository<Product> productRepo,
            IRepository<ProductImage> imageRepo,
            IRepository<Store> storeRepo,
            IRepository<ProductUnit> unitRepo,
            IRepository<Market> marketRepo,
            IRepository<User> userRepo,
            IMarketService marketService,
            IMapper mapper,
            IMongoDatabase database)
        {
            _productRepo = productRepo;
            _imageRepo = imageRepo;
            _storeRepo = storeRepo;
            _unitRepo = unitRepo;
            _marketRepo = marketRepo;
            _userRepo = userRepo;
            _marketService = marketService;
            _mapper = mapper;
            _productCollection = database.GetCollection<Product>("Products");
            _productImageCollection = database.GetCollection<ProductImage>("ProductImages");
            _storeCollection = database.GetCollection<Store>("Stores");
            _unitCollection = database.GetCollection<ProductUnit>("ProductUnits");
            _marketCollection = database.GetCollection<Market>("Markets");
            _userCollection = database.GetCollection<User>("Users");
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

        // UC049: View All Product List (FOR BUYERS - ALL STATUSES)
        public async Task<PagedResultDto<ProductDto>> GetAllProductsAsync(int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Max(pageSize, 1);

            var activeMarkets = await _marketCollection
                .Find(m => m.Status == "Active")
                .ToListAsync();

            var activeMarketIds = activeMarkets
                .Select(m => m.Id)
                .Where(id => !string.IsNullOrEmpty(id))
                .Cast<string>()
                .ToList();

            if (activeMarketIds.Count == 0)
            {
                return new PagedResultDto<ProductDto>
                {
                    Items = Array.Empty<ProductDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }

            var validStores = await _storeCollection
                .Find(s => s.Status == "Open" && activeMarketIds.Contains(s.MarketId))
                .ToListAsync();

            var validStoreIds = validStores
                .Select(s => s.Id)
                .Where(id => !string.IsNullOrEmpty(id))
                .Cast<string>()
                .ToList();

            if (validStoreIds.Count == 0)
            {
                return new PagedResultDto<ProductDto>
                {
                    Items = Array.Empty<ProductDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }

            var allowedStatuses = new[]
            {
                ProductStatus.Active,
                ProductStatus.OutOfStock,
                ProductStatus.Inactive,
                ProductStatus.Suspended
            };

            var productFilter = Builders<Product>.Filter.In(p => p.StoreId, validStoreIds)
                & Builders<Product>.Filter.In(p => p.Status, allowedStatuses);

            var totalTask = _productCollection.CountDocumentsAsync(productFilter);
            var productsTask = _productCollection
                .Find(productFilter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            await Task.WhenAll(totalTask, productsTask);

            var items = await BuildEnrichedProductDtosAsync(await productsTask);

            return new PagedResultDto<ProductDto>
            {
                Items = items,
                TotalCount = (int)await totalTask,
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
                    unitName = unit.DisplayName;
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

        // UC055: Filter Products (FOR BUYERS - ALL STATUSES WITH FILTER)
        public async Task<PagedResultDto<ProductDto>> FilterProductsAsync(ProductFilterDto filter)
        {
            filter.Page = Math.Max(filter.Page, 1);
            filter.PageSize = Math.Max(filter.PageSize, 1);

            var productFilters = new List<FilterDefinition<Product>>();
            var restrictToOpenStores = string.IsNullOrEmpty(filter.Status)
                || filter.Status.Equals("Active", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(filter.Status))
            {
                if (!Enum.TryParse<ProductStatus>(filter.Status, true, out var parsedStatus))
                {
                    return new PagedResultDto<ProductDto>
                    {
                        Items = Array.Empty<ProductDto>(),
                        TotalCount = 0,
                        Page = filter.Page,
                        PageSize = filter.PageSize
                    };
                }

                productFilters.Add(Builders<Product>.Filter.Eq(p => p.Status, parsedStatus));
            }
            else
            {
                productFilters.Add(Builders<Product>.Filter.Eq(p => p.Status, ProductStatus.Active));
            }

            if (!string.IsNullOrWhiteSpace(filter.CategoryId))
                productFilters.Add(Builders<Product>.Filter.Eq(p => p.CategoryId, filter.CategoryId));

            if (filter.MinPrice.HasValue)
                productFilters.Add(Builders<Product>.Filter.Gte(p => p.Price, filter.MinPrice.Value));

            if (filter.MaxPrice.HasValue)
                productFilters.Add(Builders<Product>.Filter.Lte(p => p.Price, filter.MaxPrice.Value));

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keywordRegex = new MongoDB.Bson.BsonRegularExpression(filter.Keyword.Trim(), "i");
                productFilters.Add(
                    Builders<Product>.Filter.Or(
                        Builders<Product>.Filter.Regex(p => p.Name, keywordRegex),
                        Builders<Product>.Filter.Regex(p => p.Description, keywordRegex)));
            }

            var candidateStoreIds = await ResolveCandidateStoreIdsAsync(filter, restrictToOpenStores);
            if (candidateStoreIds != null)
            {
                if (candidateStoreIds.Count == 0)
                {
                    return new PagedResultDto<ProductDto>
                    {
                        Items = Array.Empty<ProductDto>(),
                        TotalCount = 0,
                        Page = filter.Page,
                        PageSize = filter.PageSize
                    };
                }

                productFilters.Add(Builders<Product>.Filter.In(p => p.StoreId, candidateStoreIds));
            }

            var combinedFilter = productFilters.Count == 0
                ? Builders<Product>.Filter.Empty
                : Builders<Product>.Filter.And(productFilters);

            var sortDefinition = BuildProductSortDefinition(filter);
            var findQuery = _productCollection.Find(combinedFilter);
            if (sortDefinition != null)
                findQuery = findQuery.Sort(sortDefinition);

            var totalTask = _productCollection.CountDocumentsAsync(combinedFilter);
            var productsTask = findQuery
                .Skip((filter.Page - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            await Task.WhenAll(totalTask, productsTask);

            var items = await BuildEnrichedProductDtosAsync(await productsTask);

            return new PagedResultDto<ProductDto>
            {
                Items = items,
                TotalCount = (int)await totalTask,
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
                (p.Status == ProductStatus.Active || p.Status == ProductStatus.OutOfStock || p.Status == ProductStatus.Suspended)
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
            var productList = products.ToList();
            if (!productList.Any())
                return Enumerable.Empty<ProductDto>();

            // Batch load all images for all products in ONE query (fix N+1)
            var productIds = productList.Select(p => p.Id).Where(id => id != null).ToList();
            var allImages = await _imageRepo.FindManyAsync(i => productIds.Contains(i.ProductId));
            var imagesByProductId = allImages.GroupBy(i => i.ProductId)
                                              .ToDictionary(g => g.Key, g => g.Select(i => i.ImageUrl).ToList());

            var result = new List<ProductDto>();
            foreach (var product in productList)
            {
                var dto = _mapper.Map<ProductDto>(product);
                dto.ImageUrls = product.Id != null && imagesByProductId.TryGetValue(product.Id, out var urls) 
                    ? urls 
                    : new List<string>();
                result.Add(dto);
            }
            return result;
        }

        private async Task<List<ProductDto>> BuildEnrichedProductDtosAsync(IReadOnlyCollection<Product> products)
        {
            if (products.Count == 0)
                return new List<ProductDto>();

            var productIds = products
                .Select(p => p.Id)
                .Where(id => !string.IsNullOrEmpty(id))
                .Cast<string>()
                .ToList();
            var storeIds = products
                .Select(p => p.StoreId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();
            var unitIds = products
                .Select(p => p.UnitId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            var storesTask = storeIds.Count == 0
                ? Task.FromResult(new List<Store>())
                : _storeCollection.Find(s => storeIds.Contains(s.Id!)).ToListAsync();
            var unitsTask = unitIds.Count == 0
                ? Task.FromResult(new List<ProductUnit>())
                : _unitCollection.Find(u => unitIds.Contains(u.Id!)).ToListAsync();
            var imagesTask = productIds.Count == 0
                ? Task.FromResult(new List<ProductImage>())
                : _productImageCollection.Find(i => productIds.Contains(i.ProductId)).ToListAsync();

            await Task.WhenAll(storesTask, unitsTask, imagesTask);

            var stores = await storesTask;
            var units = await unitsTask;
            var images = await imagesTask;

            var marketIds = stores
                .Select(s => s.MarketId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();
            var sellerIds = stores
                .Select(s => s.SellerId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            var marketsTask = marketIds.Count == 0
                ? Task.FromResult(new List<Market>())
                : _marketCollection.Find(m => marketIds.Contains(m.Id!)).ToListAsync();
            var usersTask = sellerIds.Count == 0
                ? Task.FromResult(new List<User>())
                : _userCollection.Find(u => sellerIds.Contains(u.Id!)).ToListAsync();

            await Task.WhenAll(marketsTask, usersTask);

            var storeDict = stores.Where(s => !string.IsNullOrEmpty(s.Id)).ToDictionary(s => s.Id!, s => s);
            var unitDict = units.Where(u => !string.IsNullOrEmpty(u.Id)).ToDictionary(u => u.Id!, u => u);
            var marketDict = (await marketsTask).Where(m => !string.IsNullOrEmpty(m.Id)).ToDictionary(m => m.Id!, m => m.Name);
            var userDict = (await usersTask).Where(u => !string.IsNullOrEmpty(u.Id)).ToDictionary(u => u.Id!, u => u);
            var imageDict = images
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Select(i => i.ImageUrl).ToList());

            var result = new List<ProductDto>(products.Count);
            foreach (var product in products)
            {
                var dto = _mapper.Map<ProductDto>(product);
                dto.ImageUrls = product.Id != null && imageDict.TryGetValue(product.Id, out var urls)
                    ? urls
                    : new List<string>();

                if (!string.IsNullOrEmpty(dto.StoreId) && storeDict.TryGetValue(dto.StoreId, out var store))
                {
                    dto.StoreName = store.Name;

                    if (!string.IsNullOrEmpty(store.MarketId) && marketDict.TryGetValue(store.MarketId, out var marketName))
                        dto.MarketName = marketName;

                    if (!string.IsNullOrEmpty(store.SellerId) && userDict.TryGetValue(store.SellerId, out var seller))
                    {
                        dto.Seller = new SellerDto
                        {
                            Name = seller.FullName,
                            Rating = 0,
                            Market = dto.MarketName
                        };
                    }
                }

                if (!string.IsNullOrEmpty(dto.UnitId) && unitDict.TryGetValue(dto.UnitId, out var unit))
                    dto.UnitName = string.IsNullOrEmpty(unit.DisplayName) ? unit.Name : unit.DisplayName;

                dto.Status = product.Status;
                result.Add(dto);
            }

            return result;
        }

        private async Task<List<string>?> ResolveCandidateStoreIdsAsync(ProductFilterDto filter, bool restrictToOpenStores)
        {
            if (!restrictToOpenStores && string.IsNullOrWhiteSpace(filter.StoreId) && string.IsNullOrWhiteSpace(filter.MarketId)
                && !(filter.Latitude.HasValue && filter.Longitude.HasValue && filter.MaxDistanceKm > 0))
            {
                return null;
            }

            var storeFilters = new List<FilterDefinition<Store>>();
            if (restrictToOpenStores)
            {
                var activeMarketIds = await _marketCollection
                    .Find(m => m.Status == "Active")
                    .Project(m => m.Id)
                    .ToListAsync();

                var validMarketIds = activeMarketIds.Where(id => !string.IsNullOrEmpty(id)).Cast<string>().ToList();
                if (validMarketIds.Count == 0)
                    return new List<string>();

                storeFilters.Add(Builders<Store>.Filter.Eq(s => s.Status, "Open"));
                storeFilters.Add(Builders<Store>.Filter.In(s => s.MarketId, validMarketIds));
            }

            if (!string.IsNullOrWhiteSpace(filter.MarketId))
                storeFilters.Add(Builders<Store>.Filter.Eq(s => s.MarketId, filter.MarketId));

            if (!string.IsNullOrWhiteSpace(filter.StoreId))
                storeFilters.Add(Builders<Store>.Filter.Eq(s => s.Id, filter.StoreId));

            var storeFilter = storeFilters.Count == 0
                ? Builders<Store>.Filter.Empty
                : Builders<Store>.Filter.And(storeFilters);

            var stores = await _storeCollection.Find(storeFilter).ToListAsync();

            if (filter.Latitude.HasValue && filter.Longitude.HasValue && filter.MaxDistanceKm > 0)
            {
                stores = stores.Where(store =>
                    GetDistanceKm(filter.Latitude.Value, filter.Longitude.Value, store.Latitude, store.Longitude) <= filter.MaxDistanceKm)
                    .ToList();
            }

            return stores
                .Select(s => s.Id)
                .Where(id => !string.IsNullOrEmpty(id))
                .Cast<string>()
                .Distinct()
                .ToList();
        }

        private static SortDefinition<Product>? BuildProductSortDefinition(ProductFilterDto filter)
        {
            if (string.IsNullOrWhiteSpace(filter.SortBy))
                return null;

            var ascending = filter.Ascending != false;
            return filter.SortBy.Trim().ToLowerInvariant() switch
            {
                "price" => ascending
                    ? Builders<Product>.Sort.Ascending(p => p.Price)
                    : Builders<Product>.Sort.Descending(p => p.Price),
                "name" => ascending
                    ? Builders<Product>.Sort.Ascending(p => p.Name)
                    : Builders<Product>.Sort.Descending(p => p.Name),
                "status" => ascending
                    ? Builders<Product>.Sort.Ascending(p => p.Status)
                    : Builders<Product>.Sort.Descending(p => p.Status),
                "created" => ascending
                    ? Builders<Product>.Sort.Ascending(p => p.CreatedAt)
                    : Builders<Product>.Sort.Descending(p => p.CreatedAt),
                "updated" => ascending
                    ? Builders<Product>.Sort.Ascending(p => p.UpdatedAt)
                    : Builders<Product>.Sort.Descending(p => p.UpdatedAt),
                _ => null
            };
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