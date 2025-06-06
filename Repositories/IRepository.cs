using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LocalMartOnline.Repositories
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        Task CreateAsync(T entity);
        Task UpdateAsync(string id, T entity);
        Task DeleteAsync(string id);
        Task<T?> FindOneAsync(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> FindManyAsync(Expression<Func<T, bool>> filter);
    }
} 