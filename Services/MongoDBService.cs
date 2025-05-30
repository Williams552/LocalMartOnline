using MongoDB.Driver;
using LocalMartOnline.Models;

namespace LocalMartOnline.Services
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;

        public MongoDBService(IMongoDatabase database)
        {
            _database = database;
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
} 