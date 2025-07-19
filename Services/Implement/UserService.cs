using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using LocalMartOnline.Models.DTOs.User;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver;

namespace LocalMartOnline.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepo;
        private readonly AutoMapper.IMapper _mapper;
        public UserService(IRepository<User> userRepo, AutoMapper.IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }
        public Task<IEnumerable<User>> GetAllAsync() => _userRepo.GetAllAsync();
        public Task<User?> GetByIdAsync(string id) => _userRepo.GetByIdAsync(id);
        public async Task CreateAsync(User user)
        {
            // Check duplicate username
            var existingByUsername = await _userRepo.FindOneAsync(u => u.Username == user.Username);
            if (existingByUsername != null)
                throw new System.Exception("Username already exists");

            // Check duplicate email
            var existingByEmail = await _userRepo.FindOneAsync(u => u.Email == user.Email);
            if (existingByEmail != null)
                throw new System.Exception("Email already exists");

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

            // Sử dụng AutoMapper để map các trường từ DTO sang user hiện tại (trừ Password)

            var tempPassword = updateDto.Password;
            updateDto.Password = ""; // Không map password qua AutoMapper
            _mapper.Map(updateDto, currentUser);

            // Nếu có password mới, hash và gán vào user
            if (!string.IsNullOrEmpty(tempPassword))
            {
                currentUser.PasswordHash = PasswordHashService.HashPassword(tempPassword);
            }

            currentUser.UpdatedAt = DateTime.UtcNow;
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
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(userId, user);
            return true;
        }

        public async Task<bool> UpdateThemeAsync(string userId, string theme)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;
            user.PreferredTheme = theme;
            user.UpdatedAt = DateTime.UtcNow;
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
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(id, user);
            return true;
        }

        public async Task<bool> DisableOwnAccountAsync(string userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;
            user.Status = "Disabled";
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(userId, user);
            return true;
        }
    }
}
