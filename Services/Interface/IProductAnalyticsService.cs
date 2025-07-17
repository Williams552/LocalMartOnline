using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IProductAnalyticsService
    {
        Task IncrementPurchaseCountForOrderAsync(string orderId);
        Task IncrementPurchaseCountForProxyOrderAsync(string proxyOrderId);
    }
}
