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
    }
}
