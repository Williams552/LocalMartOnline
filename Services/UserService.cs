using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver;

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
    }
}
