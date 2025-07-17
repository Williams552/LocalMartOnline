using LocalMartOnline.Models.DTOs.Order;
using LocalMartOnline.Models.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IOrderService
    {
        Task<OrderDto> PlaceOrderAsync(OrderCreateDto dto); // UC070
        Task<PagedResultDto<OrderDto>> GetOrderListAsync(string buyerId, int page, int pageSize); // UC071
        Task<PagedResultDto<OrderDto>> FilterOrderListAsync(OrderFilterDto filter); // UC072
        Task<PagedResultDto<OrderDto>> GetOrderListBySellerAsync(string sellerId, int page, int pageSize);
        Task<bool> CompleteOrderAsync(string orderId); // Xác thực đơn hàng thành công và tăng PurchaseCount
    }
}