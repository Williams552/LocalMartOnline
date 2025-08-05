using LocalMartOnline.Models.DTOs.Order;
using LocalMartOnline.Models.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IOrderService
    {
        Task<OrderDto> PlaceOrderAsync(OrderCreateDto dto); // UC070
        Task<List<OrderDto>> PlaceOrdersFromCartAsync(CartOrderCreateDto dto);
        Task<PagedResultDto<OrderDto>> GetOrderListAsync(string buyerId, int page, int pageSize); // UC071
        Task<PagedResultDto<OrderDto>> FilterOrderListAsync(OrderFilterDto filter); // UC072
        Task<PagedResultDto<OrderDto>> GetOrderListBySellerAsync(string sellerId, int page, int pageSize);
        Task<PagedResultDto<OrderDto>> GetAllOrdersAsync(int page, int pageSize);
        Task<bool> CompleteOrderAsync(string orderId, string buyerId); // Buyer xác nhận đã nhận hàng
        Task<bool> CancelOrderAsync(string orderId, string userId, OrderCancelDto cancelDto); // Hủy đơn hàng
        Task<bool> ConfirmOrderAsync(string orderId, string sellerId); // Seller xác nhận còn hàng
        Task<bool> MarkAsPaidAsync(string orderId, string sellerId); // Seller xác nhận đã nhận tiền
        Task<OrderDto?> GetOrderDetailAsync(string orderId);
    }
}