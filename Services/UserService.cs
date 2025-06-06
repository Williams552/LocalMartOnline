using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace LocalMartOnline.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepo;
        public UserService(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }
        public Task<IEnumerable<User>> GetAllAsync() => _userRepo.GetAllAsync();
        public Task<User?> GetByIdAsync(string id) => _userRepo.GetByIdAsync(id);
        public Task CreateAsync(User user) => _userRepo.CreateAsync(user);
        public Task UpdateAsync(string id, User user) => _userRepo.UpdateAsync(id, user);
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
            IEnumerable<User> users = await _userRepo.GetAllAsync();
            if (!string.IsNullOrEmpty(search))
                users = users.Where(u => (u.Username?.Contains(search, System.StringComparison.OrdinalIgnoreCase) ?? false)
                                 || (u.Email?.Contains(search, System.StringComparison.OrdinalIgnoreCase) ?? false)
                                 || (u.FullName?.Contains(search, System.StringComparison.OrdinalIgnoreCase) ?? false));
            if (!string.IsNullOrEmpty(role))
                users = users.Where(u => u.Role == role);

            if (!string.IsNullOrEmpty(sortField))
            {
                bool desc = sortOrder?.ToLower() == "desc";
                users = sortField.ToLower() switch
                {
                    "username" => desc ? users.OrderByDescending(u => u.Username) : users.OrderBy(u => u.Username),
                    "email" => desc ? users.OrderByDescending(u => u.Email) : users.OrderBy(u => u.Email),
                    "fullname" => desc ? users.OrderByDescending(u => u.FullName) : users.OrderBy(u => u.FullName),
                    "createdat" => desc ? users.OrderByDescending(u => u.CreatedAt) : users.OrderBy(u => u.CreatedAt),
                    _ => users.OrderBy(u => u.Username)
                };
            }
            else
            {
                users = users.OrderBy(u => u.Username);
            }

            var total = users.Count();
            var pageData = users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return (pageData, total);
        }
    }
}
