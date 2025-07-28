using System.Threading.Tasks;
using LocalMartOnline.Models.DTOs.Shopper;

namespace LocalMartOnline.Services.Interface
{
    public interface IShopperAnalyticsService
    {
        Task<ShopperAnalyticsDto> GetShopperAnalyticsAsync(string shopperId, string period);
    }
}