using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Models.DTOs.Store;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Implement
{
    public class StoreService : IStoreService
    {
        private readonly IRepository<Store> _storeRepo;
        private readonly IRepository<StoreFollow> _followRepo;
        private readonly IRepository<MarketFeePayment> _paymentRepo;
        private readonly IRepository<MarketFee> _marketFeeRepo;
        private readonly IRepository<MarketFeeType> _marketFeeTypeRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Market> _marketRepo;
        private readonly IMapper _mapper;
        private readonly IMarketService _marketService;
        private readonly IRepository<Product> _productRepo; // Thêm repository này
        private readonly IRepository<Order> _orderRepo; // Thêm repository này
        private readonly IRepository<Review> _reviewRepo;

        public StoreService(
         IRepository<Store> storeRepo,
         IRepository<StoreFollow> followRepo,
         IRepository<MarketFeePayment> paymentRepo,
         IRepository<MarketFee> marketFeeRepo,
         IRepository<MarketFeeType> marketFeeTypeRepo,
         IRepository<User> userRepo,
         IRepository<Market> marketRepo,
         IRepository<Product> productRepo, // Thêm vào constructor
         IRepository<Order> orderRepo, // Thêm vào constructor
         IRepository<Review> reviewRepo, // Thêm vào constructor (nếu có)
         IMapper mapper,
         IMarketService marketService)
        {
            _storeRepo = storeRepo;
            _followRepo = followRepo;
            _paymentRepo = paymentRepo;
            _marketFeeRepo = marketFeeRepo;
            _marketFeeTypeRepo = marketFeeTypeRepo;
            _userRepo = userRepo;
            _marketRepo = marketRepo;
            _productRepo = productRepo; // Khởi tạo
            _orderRepo = orderRepo; // Khởi tạo
            _reviewRepo = reviewRepo; // Khởi tạo (nếu có)
            _mapper = mapper;
            _marketService = marketService;
        }

        // UC030: Create Store
        public async Task<StoreDto> CreateStoreAsync(StoreCreateDto dto)
        {
            // Kiểm tra xem seller đã có store chưa
            if (await HasExistingStoreAsync(dto.SellerId))
            {
                throw new InvalidOperationException("Mỗi người bán chỉ được mở một cửa hàng");
            }

            var store = _mapper.Map<Store>(dto);
            store.Status = "Open";
            store.CreatedAt = DateTime.Now;
            store.UpdatedAt = DateTime.Now;
            store.Rating = 0.0m;
            await _storeRepo.CreateAsync(store);

            return _mapper.Map<StoreDto>(store);
        }

        // UC031: Close Store
        public async Task<bool> CloseStoreAsync(string id)
        {
            var store = await _storeRepo.GetByIdAsync(id);
            if (store == null) return false;
            if (store.Status == "Closed") return true;
            store.Status = "Closed";
            store.UpdatedAt = DateTime.Now;
            await _storeRepo.UpdateAsync(id, store);
            return true;
        }

        // UC032: Update Store
        public async Task<bool> UpdateStoreAsync(string id, StoreUpdateDto dto)
        {
            var store = await _storeRepo.GetByIdAsync(id);
            if (store == null) return false;
            
            // Chỉ cập nhật những trường có giá trị trong DTO
            if (!string.IsNullOrEmpty(dto.Name))
            {
                store.Name = dto.Name;
            }
            
            if (!string.IsNullOrEmpty(dto.Address))
            {
                store.Address = dto.Address;
            }
            
            if (dto.Latitude != 0)
            {
                store.Latitude = dto.Latitude;
            }
            
            if (dto.Longitude != 0)
            {
                store.Longitude = dto.Longitude;
            }
            
            if (!string.IsNullOrEmpty(dto.ContactNumber))
            {
                store.ContactNumber = dto.ContactNumber;
            }
            
            if (!string.IsNullOrEmpty(dto.StoreImageUrl))
            {
                store.StoreImageUrl = dto.StoreImageUrl;
            }
            
            if (!string.IsNullOrEmpty(dto.MarketId))
            {
                store.MarketId = dto.MarketId;
            }
            
            store.UpdatedAt = DateTime.Now;
            await _storeRepo.UpdateAsync(id, store);
            return true;
        }

        // UC037: Follow Store
        public async Task<bool> FollowStoreAsync(string userId, string storeId)
        {
            // Kiểm tra store có tồn tại không
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store == null) return false;

            // Check if already followed
            var follows = await _followRepo.FindManyAsync(f => f.UserId == userId && f.StoreId == storeId);
            if (follows.Any()) return false;

            var follow = new StoreFollow
            {
                UserId = userId,
                StoreId = storeId,
                CreatedAt = DateTime.Now
            };
            await _followRepo.CreateAsync(follow);
            return true;
        }

        // UC039: Unfollow Store
        public async Task<bool> UnfollowStoreAsync(string userId, string storeId)
        {
            var follows = await _followRepo.FindManyAsync(f => f.UserId == userId && f.StoreId == storeId);
            var follow = follows.FirstOrDefault();
            if (follow == null) return false;
            await _followRepo.DeleteAsync(follow.Id!);
            return true;
        }

        // UC038: View Following Store List
        public async Task<IEnumerable<StoreDto>> GetFollowingStoresAsync(string userId)
        {
            var follows = await _followRepo.FindManyAsync(f => f.UserId == userId);
            var storeIds = follows.Select(f => f.StoreId).ToList();
            if (!storeIds.Any()) return Enumerable.Empty<StoreDto>();

            // Tìm theo Id (ObjectId dạng string)
            var stores = await _storeRepo.FindManyAsync(s => storeIds.Contains(s.Id!));
            return _mapper.Map<IEnumerable<StoreDto>>(stores);
        }

        // Check if user is following a store
        public async Task<bool> IsFollowingStoreAsync(string userId, string storeId)
        {
            var follows = await _followRepo.FindManyAsync(f => f.UserId == userId && f.StoreId == storeId);
            return follows.Any();
        }

        // Get store followers with pagination
        public async Task<PagedResultDto<object>> GetStoreFollowersAsync(string storeId, int page, int pageSize)
        {
            var follows = await _followRepo.FindManyAsync(f => f.StoreId == storeId);
            var totalCount = follows.Count();

            var pagedFollows = follows
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Trả về thông tin cơ bản của followers
            var followers = pagedFollows.Select(f => new
            {
                UserId = f.UserId,
                FollowedAt = f.CreatedAt
            }).ToList();

            return new PagedResultDto<object>
            {
                Items = followers.Cast<object>().ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // UC040: View Store Profile
        public async Task<StoreDto?> GetStoreProfileAsync(string id)
        {
            var store = await _storeRepo.GetByIdAsync(id);
            return store == null ? null : _mapper.Map<StoreDto>(store);
        }

        public async Task<PagedResultDto<StoreDto>> GetAllStoresAsync(int page, int pageSize)
        {
            var stores = await _storeRepo.GetAllAsync();
            var total = stores.Count();
            var paged = stores
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
            var items = _mapper.Map<IEnumerable<StoreDto>>(paged);
            return new PagedResultDto<StoreDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
        // Sửa lại method GetStoresBySellerAsync để trả về 1 store duy nhất
        public async Task<StoreDto?> GetStoreBySellerAsync(string sellerId)
        {
            try
            {
                var stores = await _storeRepo.FindManyAsync(s => s.SellerId == sellerId);
                var store = stores.FirstOrDefault(); // Lấy store đầu tiên (chỉ có 1)

                if (store == null)
                    return null;

                return _mapper.Map<StoreDto>(store);
            }
            catch (Exception ex)
            {
                // Log error nếu cần
                throw new InvalidOperationException($"Lỗi khi lấy thông tin gian hàng: {ex.Message}");
            }
        }

        public async Task<PagedResultDto<StoreDto>> GetSuspendedStoresAsync(int page, int pageSize)
        {
            var stores = await _storeRepo.FindManyAsync(s => s.Status == "Suspended");
            var total = stores.Count();
            var paged = stores
                .OrderByDescending(s => s.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
            var items = _mapper.Map<IEnumerable<StoreDto>>(paged);
            return new PagedResultDto<StoreDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> SuspendStoreAsync(string id, string reason)
        {
            var store = await _storeRepo.GetByIdAsync(id);
            if (store == null) return false;
            if (store.Status == "Suspended") return true;
            store.Status = "Suspended";
            store.UpdatedAt = DateTime.Now;
            // Optionally, you can add a property to store the reason if your model supports it
            await _storeRepo.UpdateAsync(id, store);
            return true;
        }

        public async Task<bool> ReactivateStoreAsync(string id)
        {
            var store = await _storeRepo.GetByIdAsync(id);
            if (store == null) return false;
            if (store.Status != "Suspended") return true;
            store.Status = "Open";
            store.UpdatedAt = DateTime.Now;
            await _storeRepo.UpdateAsync(id, store);
            return true;
        }

        public async Task<PagedResultDto<StoreDto>> GetActiveStoresByMarketIdAsync(string marketId, int page, int pageSize)
        {
            // Tìm các store đang active (trạng thái "Open") thuộc market này
            var stores = await _storeRepo.FindManyAsync(s => 
                s.MarketId.ToString() == marketId && 
                s.Status == "Open" );
            
            var total = stores.Count();
            var paged = stores
                .OrderByDescending(s => s.Rating) // Sắp xếp theo rating cao nhất
                .ThenByDescending(s => s.CreatedAt) // Sau đó theo thời gian tạo mới nhất
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
                
            var items = _mapper.Map<IEnumerable<StoreDto>>(paged);
            
            return new PagedResultDto<StoreDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> ToggleStoreStatusAsync(string id)
        {
            var store = await _storeRepo.GetByIdAsync(id);
            if (store == null) return false;
            
            // Nếu store đang Suspended, không cho phép toggle
            if (store.Status == "Suspended")
                return false;
            
            // Nếu đang chuyển từ Closed sang Open, kiểm tra market có đang hoạt động không
            if (store.Status == "Closed")
            {
                var isMarketOpen = await _marketService.IsMarketOpenAsync(store.MarketId);
                if (!isMarketOpen)
                {
                    // Market đang đóng cửa, không cho phép mở store
                    return false;
                }
                store.Status = "Open";
            }
            else if (store.Status == "Open")
            {
                // Luôn cho phép đóng store
                store.Status = "Closed";
            }
            else
            {
                return false; // Trạng thái không hợp lệ
            }
                
            store.UpdatedAt = DateTime.Now;
            await _storeRepo.UpdateAsync(id, store);
            return true;
        }

        public async Task<(bool Success, string Message)> ToggleStoreStatusWithMessageAsync(string id)
        {
            var store = await _storeRepo.GetByIdAsync(id);
            if (store == null) 
                return (false, "Không tìm thấy cửa hàng");
            
            // Nếu store đang Suspended, không cho phép toggle
            if (store.Status == "Suspended")
                return (false, "Cửa hàng đang bị tạm ngưng, không thể thay đổi trạng thái");
            
            // Nếu đang chuyển từ Closed sang Open, kiểm tra market có đang hoạt động không
            if (store.Status == "Closed")
            {
                var (isMarketOpen, reason) = await _marketService.GetMarketOpenStatusAsync(store.MarketId);
                if (!isMarketOpen)
                {
                    return (false, $"Không thể mở cửa hàng: {reason}");
                }
                store.Status = "Open";
            }
            else if (store.Status == "Open")
            {
                // Luôn cho phép đóng store
                store.Status = "Closed";
            }
            else
            {
                return (false, "Trạng thái cửa hàng không hợp lệ");
            }
                
            store.UpdatedAt = DateTime.Now;
            await _storeRepo.UpdateAsync(id, store);
            
            string action = store.Status == "Open" ? "mở" : "đóng";
            return (true, $"Đã {action} cửa hàng thành công");
        }

        public async Task<PagedResultDto<StoreDto>> SearchStoresAsync(StoreSearchFilterDto filter, bool isAdmin = false)
        {
            
            Expression<Func<Store, bool>> searchExpression = s => true;

            // Nếu không phải admin, chỉ tìm kiếm store đang mở cửa
            if (!isAdmin)
            {
                searchExpression = s => s.Status == "Open";
            }
            else if (!string.IsNullOrEmpty(filter.Status))
            {
                // Admin có thể lọc theo trạng thái
                searchExpression = s => s.Status == filter.Status;
            }

            // Thực hiện truy vấn cơ bản
            var stores = await _storeRepo.FindManyAsync(searchExpression);

            // Lọc bằng LINQ trong memory
            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                stores = stores.Where(s => s.Name.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(filter.MarketId))
            {
                stores = stores.Where(s => s.MarketId.ToString() == filter.MarketId).ToList();
            }

            if (filter.MinRating.HasValue)
            {
                stores = stores.Where(s => s.Rating >= filter.MinRating.Value).ToList();
            }

            if (filter.MaxRating.HasValue)
            {
                stores = stores.Where(s => s.Rating <= filter.MaxRating.Value).ToList();
            }

            // Lọc theo khoảng cách nếu có tọa độ
            if (filter.Latitude.HasValue && filter.Longitude.HasValue && filter.MaxDistance.HasValue)
            {
                stores = stores.Where(s =>
                    CalculateDistance(filter.Latitude.Value, filter.Longitude.Value, s.Latitude, s.Longitude)
                    <= (double)filter.MaxDistance.Value)  // Cast to double to fix error 2
                    .ToList();
            }

            // Sắp xếp kết quả
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy.ToLower())
                {
                    case "rating":
                        stores = filter.Ascending
                            ? stores.OrderBy(s => s.Rating).ToList()
                            : stores.OrderByDescending(s => s.Rating).ToList();
                        break;
                    case "distance":
                        if (filter.Latitude.HasValue && filter.Longitude.HasValue)
                        {
                            stores = stores
                                .OrderBy(s => CalculateDistance(
                                    filter.Latitude.Value, filter.Longitude.Value,
                                    s.Latitude, s.Longitude))
                                .ToList();
                        }
                        break;
                    case "created":
                    default:
                        stores = filter.Ascending
                            ? stores.OrderBy(s => s.CreatedAt).ToList()
                            : stores.OrderByDescending(s => s.CreatedAt).ToList();
                        break;
                }
            }
            else
            {
                // Mặc định sắp xếp theo rating
                stores = stores.OrderByDescending(s => s.Rating).ToList();
            }

            // Phân trang
            int totalCount = stores.Count();
            var pagedStores = stores
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            // Chuyển đổi sang DTO
            var storeDtos = _mapper.Map<IEnumerable<StoreDto>>(pagedStores);

            return new PagedResultDto<StoreDto>
            {
                Items = storeDtos,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<PagedResultDto<StoreDto>> FindStoresNearbyAsync(decimal latitude, decimal longitude,
      decimal maxDistanceKm, int page = 1, int pageSize = 20)
        {
            // Lấy tất cả cửa hàng đang mở
            var stores = await _storeRepo.FindManyAsync(s => s.Status == "Open");

            // Tính khoảng cách và lọc theo khoảng cách
            var storesWithDistance = stores
                .Select(s => new
                {
                    Store = s,
                    Distance = CalculateDistance(latitude, longitude, s.Latitude, s.Longitude)
                })
                .Where(x => x.Distance <= (double)maxDistanceKm)  // Cast to double to fix error
                .OrderBy(x => x.Distance)
                .ToList();

            // Phân trang
            int totalCount = storesWithDistance.Count;
            var pagedStores = storesWithDistance
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.Store)
                .ToList();

            // Chuyển đổi sang DTO
            var storeDtos = _mapper.Map<IEnumerable<StoreDto>>(pagedStores);

            return new PagedResultDto<StoreDto>
            {
                Items = storeDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // Phương thức tính khoảng cách giữa hai điểm (sử dụng công thức Haversine)
        private double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double R = 6371; // Bán kính trái đất tính bằng km
            var dLat = ToRadians((double)(lat2 - lat1));
            var dLon = ToRadians((double)(lon2 - lon1));
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public async Task<PagedResultDto<StoreDto>> GetActiveStoresAsync(int page, int pageSize)
        {
            // Chỉ lấy các store có trạng thái "Open"
            var stores = await _storeRepo.FindManyAsync(s => s.Status == "Open");
            var total = stores.Count();
            var paged = stores
                .OrderByDescending(s => s.Rating) // Sắp xếp theo rating cao nhất trước
                .ThenByDescending(s => s.CreatedAt) // Sau đó theo thời gian tạo mới nhất
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
            var items = _mapper.Map<IEnumerable<StoreDto>>(paged);
            return new PagedResultDto<StoreDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> HasExistingStoreAsync(string sellerId)
        {
            var stores = await _storeRepo.FindManyAsync(s => s.SellerId == sellerId);
            return stores.Any(); // Trả về true nếu seller đã có ít nhất một store
        }

        // Get Store Statistics and Featured Products
        public async Task<object> GetStoreStatisticsAsync(string storeId)
        {
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store == null) return null;

            // 1. Đếm số sản phẩm của store (chỉ Active products)
            var products = await _productRepo.FindManyAsync(p =>
                p.StoreId == storeId &&
                p.Status == ProductStatus.Active);
            var productCount = products.Count();

            // 2. Đếm số người theo dõi store
            var followers = await _followRepo.FindManyAsync(f => f.StoreId == storeId);
            var followerCount = followers.Count();

            // 3. Đếm số đơn hàng đã hoàn thành của store
            var completedOrders = await _orderRepo.FindManyAsync(o =>
                o.SellerId == store.SellerId &&
                o.Status == OrderStatus.Completed);
            var orderCount = completedOrders.Count();

            // 4. Đếm số đánh giá (nếu có model Review)
            int reviewCount = 0;
            if (_reviewRepo != null)
            {
                var reviews = await _reviewRepo.FindManyAsync(r =>
                    r.TargetType == "Store" &&
                    r.TargetId == storeId);
                reviewCount = reviews.Count();
            }

            // 5. ViewCount - có thể implement later hoặc để giá trị mặc định
            // Hiện tại chưa có model để track view count, có thể thêm sau
            var viewCount = 0; // Hoặc có thể tính từ log/analytics

            return new
            {
                productCount = productCount,
                orderCount = orderCount,
                followerCount = followerCount,
                viewCount = viewCount, // Tạm thời để 0, có thể implement sau
                rating = store.Rating,
                reviewCount = reviewCount
            };
        }
        
        public async Task<StoreStatisticsDto> GetStatisticsAsync(string? marketId = null, DateTime? periodStart = null, DateTime? periodEnd = null)
        {
            var statistics = new StoreStatisticsDto
            {
                PeriodStart = periodStart ?? DateTime.UtcNow.AddMonths(-1),
                PeriodEnd = periodEnd ?? DateTime.UtcNow,
                MarketId = marketId
            };
            
            // Get all stores (filtering will be done in memory for simplicity)
            var allStores = (await _storeRepo.GetAllAsync()).ToList();
            var allMarkets = (await _marketRepo.GetAllAsync()).ToList();

            // Apply market filter if specified
            if (!string.IsNullOrEmpty(marketId))
            {
                allStores = allStores.Where(s => s.MarketId == marketId).ToList();
            }

            // Apply date filters if specified
            if (periodStart.HasValue || periodEnd.HasValue)
            {
                if (periodStart.HasValue)
                {
                    allStores = allStores.Where(s => s.CreatedAt >= periodStart.Value).ToList();
                }
                if (periodEnd.HasValue)
                {
                    allStores = allStores.Where(s => s.CreatedAt <= periodEnd.Value).ToList();
                }
            }

            // 1. Total store count
            statistics.TotalStoreCount = allStores.Count;

            // 2. Stores by status (Open/Closed/Suspended)
            statistics.StoresByStatus = allStores
                .GroupBy(s => s.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // 3. Stores by market
            statistics.StoresByMarket = allStores
                .GroupBy(s => s.MarketId)
                .ToDictionary(g => g.Key, g => g.Count());

            // 4. Average store rating
            var storesWithRatings = allStores.Where(s => s.Rating > 0).ToList();
            statistics.AverageStoreRating = storesWithRatings.Count > 0 ?
                (double)storesWithRatings.Average(s => s.Rating) : 0;

            // 5. Store performance tiers (based on store rating)
            statistics.StorePerformanceTiers = new Dictionary<string, int>
            {
                { "Excellent (4.5-5.0)", storesWithRatings.Count(s => s.Rating >= 4.5m) },
                { "Good (4.0-4.4)", storesWithRatings.Count(s => s.Rating >= 4.0m && s.Rating < 4.5m) },
                { "Average (3.0-3.9)", storesWithRatings.Count(s => s.Rating >= 3.0m && s.Rating < 4.0m) },
                { "Below Average (2.0-2.9)", storesWithRatings.Count(s => s.Rating >= 2.0m && s.Rating < 3.0m) },
                { "Poor (0-1.9)", storesWithRatings.Count(s => s.Rating < 2.0m) }
            };

            // 6. Store performance ranking (based on rating)
            statistics.StorePerformanceRanking = allStores
                .ToDictionary(s => s.Id ?? s.Name, s => s.Rating);

            // 7. Market distribution (already calculated in #3)
            statistics.MarketDistribution = statistics.StoresByMarket;

            // 8. Revenue per store (need to calculate from orders)
            var allOrders = await _orderRepo.GetAllAsync();
            var completedOrders = allOrders.Where(o => o.Status == OrderStatus.Completed).ToList();

            // Apply date filters to orders if specified
            if (periodStart.HasValue || periodEnd.HasValue)
            {
                if (periodStart.HasValue)
                {
                    completedOrders = completedOrders.Where(o => o.CreatedAt >= periodStart.Value).ToList();
                }
                if (periodEnd.HasValue)
                {
                    completedOrders = completedOrders.Where(o => o.CreatedAt <= periodEnd.Value).ToList();
                }
            }

            // Group revenue by seller/store
            var revenueByStore = completedOrders
                .GroupBy(o => o.SellerId)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

            // Map to store IDs and convert to dictionary
            statistics.RevenuePerStore = new Dictionary<string, decimal>();
            foreach (var store in allStores)
            {
                if (revenueByStore.ContainsKey(store.SellerId))
                {
                    statistics.RevenuePerStore[store.Id ?? store.Name] = revenueByStore[store.SellerId];
                }
                else
                {
                    statistics.RevenuePerStore[store.Id ?? store.Name] = 0;
                }
            }

            // 9. Product catalog size (number of products per store)
            var products = await _productRepo.GetAllAsync();
            var productsByStore = products
                .GroupBy(p => p.StoreId)
                .ToDictionary(g => g.Key, g => g.Count());

            statistics.ProductCatalogSize = new Dictionary<string, int>();
            foreach (var store in allStores)
            {
                if (productsByStore.ContainsKey(store.Id))
                {
                    statistics.ProductCatalogSize[store.Id ?? store.Name] = productsByStore[store.Id];
                }
                else
                {
                    statistics.ProductCatalogSize[store.Id ?? store.Name] = 0;
                }
            }

            // 10. Customer engagement (number of followers per store)
            var followers = await _followRepo.GetAllAsync();
            var followersByStore = followers
                .GroupBy(f => f.StoreId)
                .ToDictionary(g => g.Key, g => g.Count());

            statistics.CustomerEngagement = new Dictionary<string, int>();
            foreach (var store in allStores)
            {
                if (followersByStore.ContainsKey(store.Id))
                {
                    statistics.CustomerEngagement[store.Id ?? store.Name] = followersByStore[store.Id];
                }
                else
                {
                    statistics.CustomerEngagement[store.Id ?? store.Name] = 0;
                }
            }

            // 11. Growth trends (cumulative total stores per day in the period)
            var growthTrends = new Dictionary<string, decimal>();
            var startDate = statistics.PeriodStart.Date;
            var endDate = statistics.PeriodEnd.Date;

            decimal cumulativeTotal = 0;
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Số cửa hàng mới trong ngày
                var dailyStores = allStores.Count(s => s.CreatedAt.Date == date);
                cumulativeTotal += dailyStores;
                growthTrends[date.ToString("yyyy-MM-dd")] = cumulativeTotal;
            }

            statistics.GrowthTrends = growthTrends;

            // 12. Add store names and market names mapping
            statistics.StoreNames = allStores.ToDictionary(s => s.Id ?? s.Name, s => s.Name);
            statistics.MarketNames = allMarkets.ToDictionary(m => m.Id ?? m.Name, m => m.Name);

            return statistics;
        }
    }
}