using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(string id);
        Task CreateAsync(UserCreateDTO user);
        Task UpdateAsync(string id, UserUpdateDTO updateDto);
        Task DeleteAsync(string id);
        Task<(IEnumerable<User> Users, int Total)> GetUsersPagingAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            string? role = null,
            string? status = null,
            string? sortField = null,
            string? sortOrder = "asc");
        Task<bool> ToggleUserAccountAsync(string id);
        Task<bool> DisableOwnAccountAsync(string userId);

        Task<bool> UpdateLanguageAsync(string userId, string language);
        Task<bool> UpdateThemeAsync(string userId, string theme);
        Task<string?> GetLanguageAsync(string userId);
        Task<string?> GetThemeAsync(string userId);
    }
}
