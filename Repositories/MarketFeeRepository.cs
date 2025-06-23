using LocalMartOnline.Models;
using LocalMartOnline.Services;
using LocalMartOnline.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;

namespace LocalMartOnline.Repositories
{
    public class MarketFeeRepository : Repository<MarketFee>
    {
        public MarketFeeRepository(MongoDBService mongoService) : base(mongoService, "MarketFees")
        {
        }
    }
}
