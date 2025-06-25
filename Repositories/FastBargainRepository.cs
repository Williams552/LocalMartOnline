using LocalMartOnline.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Repositories
{
    public interface IFastBargainRepository
    {
        Task<FastBargain> CreateAsync(FastBargain bargain);
        Task<FastBargain> GetByIdAsync(string id);
        Task<List<FastBargain>> GetByUserIdAsync(string userId);
        Task UpdateAsync(FastBargain bargain);
        Task<List<FastBargain>> GetActiveByProductIdAsync(string productId);
    }

    public class FastBargainRepository : IFastBargainRepository
    {
        private readonly IMongoCollection<FastBargain> _collection;

        public FastBargainRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<FastBargain>("FastBargains");
        }

        public async Task<FastBargain> CreateAsync(FastBargain bargain)
        {
            await _collection.InsertOneAsync(bargain);
            return bargain;
        }

        public async Task<FastBargain> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<FastBargain>> GetByUserIdAsync(string userId)
        {
            return await _collection.Find(x => x.BuyerId == userId || x.SellerId == userId).ToListAsync();
        }

        public async Task UpdateAsync(FastBargain bargain)
        {
            await _collection.ReplaceOneAsync(x => x.Id == bargain.Id, bargain);
        }

        public async Task<List<FastBargain>> GetActiveByProductIdAsync(string productId)
        {
            return await _collection.Find(x => x.ProductId == productId && x.Status == FastBargainStatus.Pending).ToListAsync();
        }
    }
}
