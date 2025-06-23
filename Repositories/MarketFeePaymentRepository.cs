using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;

namespace LocalMartOnline.Repositories
{
    public class MarketFeePaymentRepository : Repository<MarketFeePayment>
    {
        public MarketFeePaymentRepository(MongoDBService mongoService) : base(mongoService, "MarketFeePayments")
        {
        }
    }
}
