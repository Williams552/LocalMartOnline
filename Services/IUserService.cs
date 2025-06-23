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
        Task<(IEnumerable<User> Users, int Total)> GetUsersPagingAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            string? role = null,
            string? sortField = null,
            string? sortOrder = "asc");
        Task<bool> ToggleUserAccountAsync(string id);
        Task<bool> DisableOwnAccountAsync(string userId);
    }
}
