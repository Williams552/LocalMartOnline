using LocalMartOnline.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(string id);
        Task CreateAsync(User user);
        Task UpdateAsync(string id, User user);
        Task DeleteAsync(string id);
    }
}
