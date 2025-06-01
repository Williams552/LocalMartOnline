using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}
