using MongoDB.Driver;
using MongoDB.Bson;
using LocalMartOnline.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace LocalMartOnline.Repositories
{
<<<<<<< HEAD
    public class Repository<T> : IRepository<T>
=======
    public class GenericRepository<T> : IRepository<T>
>>>>>>> origin/develop
    {
        private readonly IMongoCollection<T> _collection;

        public Repository(MongoDBService mongoDBService, string collectionName)
        {
            _collection = mongoDBService.GetCollection<T>(collectionName);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(string id, T entity)
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            await _collection.DeleteOneAsync(filter);
        }

        public async Task<T?> FindOneAsync(Expression<System.Func<T, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> FindManyAsync(Expression<System.Func<T, bool>> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        // Expose the underlying IMongoCollection<T> for advanced queries
        public MongoDB.Driver.IMongoCollection<T> GetCollection()
        {
            return _collection;
        }
    }
}