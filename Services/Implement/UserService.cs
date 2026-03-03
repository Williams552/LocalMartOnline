using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using LocalMartOnline.Models.DTOs.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver;

namespace LocalMartOnline.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<UserInteraction> _userInteractionRepo;
        private readonly IRepository<Store> _storeRepo;

        private readonly AutoMapper.IMapper _mapper;
        
        public UserService(
            IRepository<User> userRepo,
            IRepository<Order> orderRepo,
            IRepository<UserInteraction> userInteractionRepo,
            IRepository<Store> storeRepo,
            AutoMapper.IMapper mapper)
        {
            _userRepo = userRepo;
            _orderRepo = orderRepo;
            _userInteractionRepo = userInteractionRepo;
            _storeRepo = storeRepo;
            _mapper = mapper;
        }
        public Task<IEnumerable<User>> GetAllAsync() => _userRepo.GetAllAsync();
        public Task<User?> GetByIdAsync(string id) => _userRepo.GetByIdAsync(id);
        public async Task CreateAsync(UserCreateDTO userDto)
        {
            // Check duplicate username
            var existingByUsername = await _userRepo.FindOneAsync(u => u.Username == userDto.Username);
            if (existingByUsername != null)
                throw new System.Exception("Username already exists");

            // Check duplicate email
            var existingByEmail = await _userRepo.FindOneAsync(u => u.Email == userDto.Email);
            if (existingByEmail != null)
                throw new System.Exception("Email already exists");

            var user = _mapper.Map<User>(userDto);
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            user.Status = "Active"; // Mặc định là Active
            user.PasswordHash = PasswordHashService.HashPassword(userDto.Password); // Hash password từ DTO
            user.IsEmailVerified = true;

            await _userRepo.CreateAsync(user);
        }

        public async Task UpdateAsync(string id, UserUpdateDTO updateDto)
        {
            var currentUser = await _userRepo.GetByIdAsync(id);
            if (currentUser == null)
                throw new System.Exception("User not found");

            // Nếu username thay đổi, kiểm tra trùng lặp
            if (!string.IsNullOrEmpty(updateDto.Username) && updateDto.Username != currentUser.Username)
            {
                var existingByUsername = await _userRepo.FindOneAsync(u => u.Username == updateDto.Username && u.Id != id);
                if (existingByUsername != null)
                    throw new System.Exception("Username already exists");
            }

            // Nếu email thay đổi, kiểm tra trùng lặp
            if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != currentUser.Email)
            {
                var existingByEmail = await _userRepo.FindOneAsync(u => u.Email == updateDto.Email && u.Id != id);
                if (existingByEmail != null)
                    throw new System.Exception("Email already exists");
            }

            // Chỉ update những field có giá trị mới (không null/empty)
            if (!string.IsNullOrEmpty(updateDto.Username))
                currentUser.Username = updateDto.Username;
            
            if (!string.IsNullOrEmpty(updateDto.Email))
                currentUser.Email = updateDto.Email;
            
            if (!string.IsNullOrEmpty(updateDto.FullName))
                currentUser.FullName = updateDto.FullName;
            
            if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                currentUser.PhoneNumber = updateDto.PhoneNumber;
            
            if (!string.IsNullOrEmpty(updateDto.Address))
                currentUser.Address = updateDto.Address;
            
            if (!string.IsNullOrEmpty(updateDto.AvatarUrl))
                currentUser.AvatarUrl = updateDto.AvatarUrl;
            
            if (!string.IsNullOrEmpty(updateDto.Role))
                currentUser.Role = updateDto.Role;
            
            if (!string.IsNullOrEmpty(updateDto.Status))
                currentUser.Status = updateDto.Status;
            
            if (updateDto.TwoFactorEnabled.HasValue)
                currentUser.TwoFactorEnabled = updateDto.TwoFactorEnabled.Value;

            // Nếu có password mới, hash và gán vào user
            if (!string.IsNullOrEmpty(updateDto.Password))
            {
                currentUser.PasswordHash = PasswordHashService.HashPassword(updateDto.Password);
            }

            currentUser.UpdatedAt = DateTime.Now;
            await _userRepo.UpdateAsync(id, currentUser);
        }
        public Task DeleteAsync(string id) => _userRepo.DeleteAsync(id);

        // Efficient user lookups
        public Task<User?> GetByUsernameAsync(string username) => _userRepo.FindOneAsync(u => u.Username == username);
        public Task<User?> GetByEmailAsync(string email) => _userRepo.FindOneAsync(u => u.Email == email);
        public Task<IEnumerable<User>> FindManyAsync(System.Linq.Expressions.Expression<System.Func<User, bool>> filter) => _userRepo.FindManyAsync(filter);

        public async Task<(IEnumerable<User> Users, int Total)> GetUsersPagingAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            string? role = null,
            string? status = null,
            string? sortField = null,
            string? sortOrder = "asc")
        {
            // Efficient MongoDB paging, filtering, sorting
            var filterBuilder = MongoDB.Driver.Builders<User>.Filter;
            var filter = filterBuilder.Empty;
            if (!string.IsNullOrEmpty(search))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(u => u.Username, new MongoDB.Bson.BsonRegularExpression(search, "i")),
                    filterBuilder.Regex(u => u.Email, new MongoDB.Bson.BsonRegularExpression(search, "i")),
                    filterBuilder.Regex(u => u.FullName, new MongoDB.Bson.BsonRegularExpression(search, "i"))
                );
                filter &= searchFilter;
            }
            if (!string.IsNullOrEmpty(role))
            {
                filter &= filterBuilder.Eq(u => u.Role, role);
            }
            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "active")
                {
                    filter &= filterBuilder.Eq(u => u.Status, "Active");
                }
                else if (status.ToLower() == "blocked")
                {
                    filter &= filterBuilder.Ne(u => u.Status, "Active");
                }
            }

            var sortBuilder = MongoDB.Driver.Builders<User>.Sort;
            MongoDB.Driver.SortDefinition<User> sort = sortBuilder.Ascending(u => u.Username);
            bool desc = sortOrder?.ToLower() == "desc";
            if (!string.IsNullOrEmpty(sortField))
            {
                switch (sortField.ToLower())
                {
                    case "username": sort = desc ? sortBuilder.Descending(u => u.Username) : sortBuilder.Ascending(u => u.Username); break;
                    case "email": sort = desc ? sortBuilder.Descending(u => u.Email) : sortBuilder.Ascending(u => u.Email); break;
                    case "fullname": sort = desc ? sortBuilder.Descending(u => u.FullName) : sortBuilder.Ascending(u => u.FullName); break;
                    case "createdat": sort = desc ? sortBuilder.Descending(u => u.CreatedAt) : sortBuilder.Ascending(u => u.CreatedAt); break;
                }
            }

            // Access the underlying IMongoCollection<User>
            var collection = ((LocalMartOnline.Repositories.Repository<User>)_userRepo).GetCollection();
            var total = (int)await collection.CountDocumentsAsync(filter);
            var pageData = await collection.Find(filter)
                .Sort(sort)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
            return (pageData, total);
        }

        public async Task<bool> UpdateLanguageAsync(string userId, string language)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;
            user.PreferredLanguage = language;
            user.UpdatedAt = DateTime.Now;
            await _userRepo.UpdateAsync(userId, user);
            return true;
        }

        public async Task<bool> UpdateThemeAsync(string userId, string theme)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;
            user.PreferredTheme = theme;
            user.UpdatedAt = DateTime.Now;
            await _userRepo.UpdateAsync(userId, user);
            return true;
        }

        public async Task<string?> GetLanguageAsync(string userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            return user?.PreferredLanguage;
        }

        public async Task<string?> GetThemeAsync(string userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            return user?.PreferredTheme;
        }

        public async Task<bool> ToggleUserAccountAsync(string id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return false;
            user.Status = user.Status == "Active" ? "Disabled" : "Active";
            user.UpdatedAt = DateTime.Now;
            await _userRepo.UpdateAsync(id, user);
            return true;
        }

        public async Task<bool> DisableOwnAccountAsync(string userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;
            user.Status = "Disabled";
            user.UpdatedAt = DateTime.Now;
            await _userRepo.UpdateAsync(userId, user);
            return true;
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync(int? periodDays = null)
        {
            var statistics = new UserStatisticsDto();
            DateTime to = DateTime.Now;
            DateTime? from = null;
            if (periodDays.HasValue)
            {
                from = to.AddDays(-periodDays.Value);
            }

            var users = await _userRepo.FindManyAsync(_ => true);
            var filteredUsers = users as User[] ?? users.ToArray();

            IEnumerable<User> periodUsers = filteredUsers;
            if (from.HasValue)
            {
                periodUsers = filteredUsers.Where(u => u.CreatedAt >= from.Value && u.CreatedAt <= to);
            }

            statistics.TotalUsers = filteredUsers.Length;
            statistics.UsersByRole = filteredUsers
                .GroupBy(u => u.Role)
                .ToDictionary(g => g.Key, g => g.Count());
            statistics.NewRegistrations = periodUsers.Count();
            statistics.ActiveUsers = filteredUsers.Count(u => u.Status == "Active");
            statistics.InactiveUsers = filteredUsers.Count(u => u.Status != "Active");

            // User growth rate
            DateTime? previousPeriodStart = from.HasValue ? from.Value.AddDays(-periodDays ?? 7) : (DateTime?)null;
            DateTime? previousPeriodEnd = from;
            var previousUsers = previousPeriodStart.HasValue && previousPeriodEnd.HasValue
                ? filteredUsers.Count(u => u.CreatedAt >= previousPeriodStart.Value && u.CreatedAt < previousPeriodEnd.Value)
                : 0;
            var currentUsers = statistics.NewRegistrations;
            statistics.UserGrowthRate = previousUsers > 0 ? ((double)(currentUsers - previousUsers) / previousUsers) * 100 : 0;

            statistics.RoleDistribution = statistics.UsersByRole;
            statistics.ActivityLevels = filteredUsers
                .GroupBy(u => u.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Registration trends: dictionary, mỗi role là một mảng trend theo ngày
            var endDate = to;
            var startDate = from ?? endDate.AddDays(-7);
            var roles = filteredUsers.Select(u => u.Role).Distinct().ToList();
            var allowedRoles = new List<string> { "Buyer", "Seller", "Proxy Shopper" };
            var roleTrends = new Dictionary<string, List<RegistrationTrendDto>>();
            foreach (var role in allowedRoles)
            {
                roleTrends[role] = new List<RegistrationTrendDto>();
            }
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                foreach (var role in allowedRoles)
                {
                    // Cumulative total users for each role up to this date
                    var total = filteredUsers.Count(u => u.CreatedAt != null && u.CreatedAt.Value.Date <= date && u.Role == role);
                    roleTrends[role].Add(new RegistrationTrendDto
                    {
                        Date = date,
                        Count = total,
                        ByRole = new Dictionary<string, int> { { role, total } }
                    });
                }
            }
            statistics.RegistrationTrendsByRole = roleTrends;

            // Top buyers/sellers/engagement metrics
            var topBuyers = await GetTopBuyersAsync(from, to);
            statistics.TopBuyers = topBuyers;
            var topSellers = await GetTopSellersAsync(from, to);
            statistics.TopSellers = topSellers;
            statistics.PeriodStart = from;
            statistics.PeriodEnd = to;
            return statistics;
        }
        
        private async Task<List<TopUserDto>> GetTopBuyersAsync(DateTime? from, DateTime? to)
        {
            // Get all completed orders in the period
            var orderFilter = Builders<Order>.Filter.Eq(o => o.Status, OrderStatus.Completed);
            if (from.HasValue)
            {
                orderFilter = Builders<Order>.Filter.And(orderFilter, Builders<Order>.Filter.Gte(o => o.CreatedAt, from.Value));
            }
            if (to.HasValue)
            {
                orderFilter = Builders<Order>.Filter.And(orderFilter, Builders<Order>.Filter.Lte(o => o.CreatedAt, to.Value));
            }
            
            var completedOrders = await _orderRepo.FindManyAsync(_ => true);
            var filteredOrders = completedOrders.Where(o => o.Status == OrderStatus.Completed);
            if (from.HasValue || to.HasValue)
            {
                filteredOrders = filteredOrders.Where(o =>
                    (!from.HasValue || o.CreatedAt >= from.Value) &&
                    (!to.HasValue || o.CreatedAt <= to.Value));
            }
            
            // Group by buyer ID and count orders
            var buyerOrderCounts = filteredOrders
                .GroupBy(o => o.BuyerId)
                .Select(g => new { BuyerId = g.Key, OrderCount = g.Count() })
                .OrderByDescending(x => x.OrderCount)
                .Take(10); // Top 10 buyers
            
            // Get user details for top buyers
            var topBuyers = new List<TopUserDto>();
            foreach (var buyer in buyerOrderCounts)
            {
                var user = await _userRepo.GetByIdAsync(buyer.BuyerId);
                if (user != null)
                {
                    topBuyers.Add(new TopUserDto
                    {
                        UserId = user.Id ?? string.Empty,
                        Username = user.Username,
                        FullName = user.FullName,
                        Role = user.Role,
                        ActivityCount = buyer.OrderCount,
                        Rating = 0 // Buyers don't have ratings in this model
                    });
                }
            }
            
            return topBuyers;
        }
        
        private async Task<List<TopUserDto>> GetTopSellersAsync(DateTime? from, DateTime? to)
        {
            // Get all completed orders in the period
            var orderFilter = Builders<Order>.Filter.Eq(o => o.Status, OrderStatus.Completed);
            if (from.HasValue)
            {
                orderFilter = Builders<Order>.Filter.And(orderFilter, Builders<Order>.Filter.Gte(o => o.CreatedAt, from.Value));
            }
            if (to.HasValue)
            {
                orderFilter = Builders<Order>.Filter.And(orderFilter, Builders<Order>.Filter.Lte(o => o.CreatedAt, to.Value));
            }
            
            var completedOrders = await _orderRepo.FindManyAsync(_ => true);
            var filteredOrders = completedOrders.Where(o => o.Status == OrderStatus.Completed);
            if (from.HasValue || to.HasValue)
            {
                filteredOrders = filteredOrders.Where(o =>
                    (!from.HasValue || o.CreatedAt >= from.Value) &&
                    (!to.HasValue || o.CreatedAt <= to.Value));
            }
            
            // Group by seller ID and count orders
            var sellerOrderCounts = filteredOrders
                .GroupBy(o => o.SellerId)
                .Select(g => new { SellerId = g.Key, OrderCount = g.Count() })
                .OrderByDescending(x => x.OrderCount)
                .Take(10); // Top 10 sellers
            
            // Get user and store details for top sellers
            var topSellers = new List<TopUserDto>();
            foreach (var seller in sellerOrderCounts)
            {
                var user = await _userRepo.GetByIdAsync(seller.SellerId);
                if (user != null)
                {
                    // Get store for this seller to get rating
                    var store = await _storeRepo
                        .FindManyAsync(s => s.SellerId == seller.SellerId);                    if (store == null) continue; // Skip if no store found for seller
                    
                    var sellerStore = store?.FirstOrDefault();
                    topSellers.Add(new TopUserDto
                    {
                        UserId = user.Id ?? string.Empty,
                        Username = user.Username,
                        FullName = user.FullName,
                        Role = user.Role,
                        ActivityCount = seller.OrderCount,
                        Rating = sellerStore?.Rating ?? 0
                    });
                }
            }
            
            return topSellers;
        }
    }
}
