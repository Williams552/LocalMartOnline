using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Order;
using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace LocalMartOnline.Services.Implement
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<OrderItem> _orderItemRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IMapper _mapper;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<ProductUnit> _productUnitCollection;
        private readonly IMongoCollection<ProductImage> _productImageCollection;
        private readonly IMongoCollection<Store> _storeCollection;
        private readonly IRepository<Notification> _notificationRepo;

        public OrderService(
            IMongoDatabase database,
            IRepository<Order> orderRepo,
            IRepository<OrderItem> orderItemRepo,
            IRepository<Product> productRepo,
            IRepository<Notification> notificationRepo,
            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _productRepo = productRepo;
            _notificationRepo = notificationRepo;
            _mapper = mapper;
            _userCollection = database.GetCollection<User>("Users");
            _productCollection = database.GetCollection<Product>("Products");
            _productUnitCollection = database.GetCollection<ProductUnit>("ProductUnits");
            _productImageCollection = database.GetCollection<ProductImage>("ProductImages");
            _storeCollection = database.GetCollection<Store>("Stores");
        }

        // UC070: Place Order
        public async Task<OrderDto> PlaceOrderAsync(OrderCreateDto dto)
        {
            var order = new Order
            {
                BuyerId = dto.BuyerId,
                SellerId = dto.SellerId,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Tính tổng tiền
            order.TotalAmount = dto.Items.Sum(i => i.PriceAtPurchase * i.Quantity);

            await _orderRepo.CreateAsync(order);

            // Lưu các order item
            foreach (var item in dto.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id!,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PriceAtPurchase = item.PriceAtPurchase
                };
                await _orderItemRepo.CreateAsync(orderItem);
            }

            // Map lại sang DTO
            var orderDto = _mapper.Map<OrderDto>(order);
            orderDto.Items = dto.Items;

            // Tạo notification cho seller
            var buyer = await _userCollection.Find(u => u.Id == dto.BuyerId).FirstOrDefaultAsync();
            var buyerName = buyer?.FullName ?? "Khách hàng";
            var store = await _storeCollection.Find(s => s.SellerId == dto.SellerId).FirstOrDefaultAsync();
            var storeName = store?.Name ?? "Cửa hàng";
            
            await CreateNewOrderNotification(dto.SellerId, order, buyerName, storeName);

            return orderDto;
        }

        // UC071: View Order List
        public async Task<PagedResultDto<OrderDto>> GetOrderListAsync(string buyerId, int page, int pageSize)
        {
            var orders = await _orderRepo.FindManyAsync(o => o.BuyerId == buyerId);
            var total = orders.Count();
            var paged = orders
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var result = new List<OrderDto>();

            foreach (var order in paged)
            {
                var dto = _mapper.Map<OrderDto>(order);

                // Lấy thông tin cửa hàng
                var store = await _storeCollection
                    .Find(s => s.SellerId == order.SellerId)
                    .FirstOrDefaultAsync();
                dto.StoreName = store?.Name ?? "Unknown Store";

                var items = await _orderItemRepo.FindManyAsync(i => i.OrderId == order.Id);
                var itemDtos = new List<OrderItemDto>();
                foreach (var item in items)
                {
                    var product = await _productCollection
                        .Find(p => p.Id == item.ProductId)
                        .FirstOrDefaultAsync();

                    if (product == null) continue;

                    var unit = await _productUnitCollection
                        .Find(u => u.Id == product.UnitId)
                        .FirstOrDefaultAsync();

                    var image = await _productImageCollection
                        .Find(img => img.ProductId == product.Id)
                        .FirstOrDefaultAsync();

                    itemDtos.Add(new OrderItemDto
                    {
                        ProductId = product.Id ?? string.Empty,
                        ProductName = product.Name,
                        ProductImageUrl = image?.ImageUrl ?? string.Empty,
                        ProductUnitName = unit?.DisplayName ?? "kg",
                        Quantity = item.Quantity,
                        PriceAtPurchase = item.PriceAtPurchase
                    });
                }

                dto.Items = itemDtos;
                result.Add(dto);
            }

            return new PagedResultDto<OrderDto>
            {
                Items = result,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }


        // UC072: Filter Order List
        public async Task<PagedResultDto<OrderDto>> FilterOrderListAsync(OrderFilterDto filter)
        {
            var orders = await _orderRepo.GetAllAsync();
            var filtered = orders.Where(o =>
                (string.IsNullOrEmpty(filter.BuyerId) || o.BuyerId == filter.BuyerId) &&
                (string.IsNullOrEmpty(filter.Status) || o.Status.ToString() == filter.Status) &&
                (string.IsNullOrEmpty(filter.PaymentStatus) || o.PaymentStatus.ToString() == filter.PaymentStatus) &&
                (!filter.FromDate.HasValue || o.CreatedAt >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || o.CreatedAt <= filter.ToDate.Value)
            ).OrderByDescending(o => o.CreatedAt).ToList();

            var total = filtered.Count();
            var paged = filtered.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);

            var result = new List<OrderDto>();
            foreach (var order in paged)
            {
                var dto = _mapper.Map<OrderDto>(order);

                // Lấy thông tin cửa hàng
                var store = await _storeCollection
                    .Find(s => s.SellerId == order.SellerId)
                    .FirstOrDefaultAsync();
                dto.StoreName = store?.Name ?? "Unknown Store";

                var items = await _orderItemRepo.FindManyAsync(i => i.OrderId == order.Id);
                dto.Items = items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    PriceAtPurchase = i.PriceAtPurchase
                }).ToList();
                result.Add(dto);
            }

            return new PagedResultDto<OrderDto>
            {
                Items = result,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<PagedResultDto<OrderDto>> GetOrderListBySellerAsync(string sellerId, int page, int pageSize)
        {
            var orders = await _orderRepo.FindManyAsync(o => o.SellerId == sellerId);
            var total = orders.Count();
            var paged = orders.OrderByDescending(o => o.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize);

            var result = new List<OrderDto>();
            foreach (var order in paged)
            {
                var dto = _mapper.Map<OrderDto>(order);

                // Lấy thông tin người mua
                var buyer = await _userCollection.Find(u => u.Id == order.BuyerId).FirstOrDefaultAsync();
                dto.BuyerName = buyer?.FullName ?? "Unknown";
                dto.BuyerPhone = buyer?.PhoneNumber ?? "Unknown";

                // Lấy thông tin cửa hàng
                var store = await _storeCollection
                    .Find(s => s.SellerId == order.SellerId)
                    .FirstOrDefaultAsync();
                dto.StoreName = store?.Name ?? "Unknown Store";

                var items = await _orderItemRepo.FindManyAsync(i => i.OrderId == order.Id);
                var itemDtos = new List<OrderItemDto>();
                foreach (var item in items)
                {
                    var product = await _productCollection
                        .Find(p => p.Id == item.ProductId)
                        .FirstOrDefaultAsync();

                    if (product == null) continue;

                    var unit = await _productUnitCollection
                        .Find(u => u.Id == product.UnitId)
                        .FirstOrDefaultAsync();

                    var image = await _productImageCollection
                        .Find(img => img.ProductId == product.Id)
                        .FirstOrDefaultAsync();

                    itemDtos.Add(new OrderItemDto
                    {
                        ProductId = product.Id ?? string.Empty,
                        ProductName = product.Name,
                        ProductImageUrl = image?.ImageUrl ?? string.Empty,
                        ProductUnitName = unit?.DisplayName ?? "kg",
                        Quantity = item.Quantity,
                        PriceAtPurchase = item.PriceAtPurchase
                    });
                }

                dto.Items = itemDtos;
                result.Add(dto);
            }

            return new PagedResultDto<OrderDto>
            {
                Items = result,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // Xác thực đơn hàng thành công và tăng PurchaseCount
        public async Task<bool> CompleteOrderAsync(string orderId)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId);
            if (order == null || order.Status == OrderStatus.Completed) return false;

            // Kiểm tra trạng thái (chỉ có thể complete khi Paid)
            if (order.Status != OrderStatus.Paid)
            {
                throw new Exception($"Không thể hoàn thành đơn hàng ở trạng thái {order.Status}");
            }

            // Cập nhật trạng thái đơn hàng
            order.Status = OrderStatus.Completed;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(orderId, order);

            // Tăng PurchaseCount cho các sản phẩm trong đơn hàng
            var orderItems = await _orderItemRepo.FindManyAsync(oi => oi.OrderId == orderId);
            var productIds = orderItems.Select(oi => oi.ProductId).Distinct().ToList();

            foreach (var productId in productIds)
            {
                var product = await _productRepo.FindOneAsync(p => p.Id == productId);
                if (product != null)
                {
                    product.PurchaseCount += 1; // Tăng 1 lần mua (không phụ thuộc số lượng)
                    await _productRepo.UpdateAsync(product.Id!, product);
                }
            }

            // Tạo notification cho seller
            await CreateOrderStatusNotification(order, "Đơn hàng đã hoàn thành", 
                "Khách hàng đã xác nhận nhận hàng thành công. Giao dịch hoàn tất.", "ORDER_COMPLETED", false);

            return true;
        }

        public async Task<PagedResultDto<OrderDto>> GetAllOrdersAsync(int page, int pageSize)
        {
            var orders = await _orderRepo.GetAllAsync();
            var total = orders.Count();
            var paged = orders.OrderByDescending(o => o.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize);

            var result = new List<OrderDto>();
            foreach (var order in paged)
            {
                var dto = _mapper.Map<OrderDto>(order);

                // Lấy thông tin người mua
                var buyer = await _userCollection.Find(u => u.Id == order.BuyerId).FirstOrDefaultAsync();
                dto.BuyerName = buyer?.FullName ?? "Unknown";
                dto.BuyerPhone = buyer?.PhoneNumber ?? "Unknown";

                // Lấy thông tin cửa hàng
                var store = await _storeCollection
                    .Find(s => s.SellerId == order.SellerId)
                    .FirstOrDefaultAsync();
                dto.StoreName = store?.Name ?? "Unknown Store";

                var items = await _orderItemRepo.FindManyAsync(i => i.OrderId == order.Id);
                var itemDtos = new List<OrderItemDto>();

                foreach (var item in items)
                {
                    var product = await _productCollection
                        .Find(p => p.Id == item.ProductId)
                        .FirstOrDefaultAsync();

                    if (product == null) continue;

                    var unit = await _productUnitCollection
                        .Find(u => u.Id == product.UnitId)
                        .FirstOrDefaultAsync();

                    var image = await _productImageCollection
                        .Find(img => img.ProductId == product.Id)
                        .FirstOrDefaultAsync();

                    itemDtos.Add(new OrderItemDto
                    {
                        ProductId = product.Id ?? string.Empty,
                        ProductName = product.Name,
                        ProductImageUrl = image?.ImageUrl ?? string.Empty,
                        ProductUnitName = unit?.DisplayName ?? "kg",
                        Quantity = item.Quantity,
                        PriceAtPurchase = item.PriceAtPurchase
                    });
                }

                dto.Items = itemDtos;
                result.Add(dto);
            }

            return new PagedResultDto<OrderDto>
            {
                Items = result,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<OrderDto>> PlaceOrdersFromCartAsync(CartOrderCreateDto dto)
        {
            var orders = new List<OrderDto>();

            // Lấy thông tin buyer một lần
            var buyer = await _userCollection.Find(u => u.Id == dto.BuyerId).FirstOrDefaultAsync();
            var buyerName = buyer?.FullName ?? "Khách hàng";

            // Nhóm các sản phẩm theo StoreId (SellerId)
            var groupedByStore = dto.CartItems.GroupBy(item => item.Product.StoreId);

            foreach (var storeGroup in groupedByStore)
            {
                var storeId = storeGroup.Key;
                var storeItems = storeGroup.ToList();

                // Lấy thông tin seller từ store
                var store = await _storeCollection
                    .Find(s => s.Id == storeId)
                    .FirstOrDefaultAsync();

                if (store == null) continue;

                // Tạo đơn hàng cho store này
                var order = new Order
                {
                    BuyerId = dto.BuyerId,
                    SellerId = store.SellerId,
                    Status = OrderStatus.Pending,
                    PaymentStatus = PaymentStatus.Pending,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Tính tổng tiền cho đơn hàng này
                order.TotalAmount = storeItems.Sum(i => i.Product.Price * (decimal)i.Quantity);

                await _orderRepo.CreateAsync(order);

                // Lưu các order item cho store này
                var orderItemDtos = new List<OrderItemDto>();
                foreach (var cartItem in storeItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id!,
                        ProductId = cartItem.ProductId,
                        Quantity = (int)cartItem.Quantity,
                        PriceAtPurchase = cartItem.Product.Price
                    };
                    await _orderItemRepo.CreateAsync(orderItem);

                    // Tạo OrderItemDto
                    orderItemDtos.Add(new OrderItemDto
                    {
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        ProductImageUrl = cartItem.Product.Images,
                        ProductUnitName = cartItem.Product.Unit,
                        Quantity = (int)cartItem.Quantity,
                        PriceAtPurchase = cartItem.Product.Price
                    });
                }

                // Map sang DTO
                var orderDto = _mapper.Map<OrderDto>(order);
                orderDto.Items = orderItemDtos;
                orderDto.StoreName = store.Name;
                orderDto.BuyerName = buyerName;

                orders.Add(orderDto);

                // Tạo notification nội bộ cho seller
                await CreateNewOrderNotification(store.SellerId, order, buyerName, store.Name);
            }
            return orders;
        }

        private async Task CreateNewOrderNotification(string sellerId, Order order, string buyerName, string storeName)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = sellerId,
                    Title = "Đơn hàng mới",
                    Message = $"Bạn có đơn hàng mới từ {buyerName} tại cửa hàng {storeName}. Giá trị: {order.TotalAmount:N0}đ. Mã đơn: #{order.Id}",
                    Type = "NEW_ORDER",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepo.CreateAsync(notification);
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw để không ảnh hưởng đến việc tạo đơn hàng
                Console.Error.WriteLine($"Failed to create notification for seller {sellerId}: {ex.Message}");
            }
        }

        public async Task<bool> CancelOrderAsync(string orderId, string userId, OrderCancelDto cancelDto)
        {
            try
            {
                // Lấy thông tin đơn hàng
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new Exception("Đơn hàng không tồn tại");
                }

                // Kiểm tra quyền hủy đơn hàng (chỉ buyer hoặc seller)
                if (order.BuyerId != userId && order.SellerId != userId)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền hủy đơn hàng này");
                }

                // Kiểm tra trạng thái đơn hàng (chỉ có thể hủy khi Pending hoặc Confirmed)
                if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                {
                    throw new Exception($"Không thể hủy đơn hàng ở trạng thái {order.Status}");
                }

                // Cập nhật trạng thái đơn hàng
                order.Status = OrderStatus.Cancelled;
                order.CancelReason = cancelDto.CancelReason;
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepo.UpdateAsync(order.Id!, order);

                // Tạo notification cho bên còn lại
                await CreateCancelOrderNotification(order, userId);

                return true;
            }
            catch
            {
                throw;
            }
        }

        private async Task CreateCancelOrderNotification(Order order, string cancelledByUserId)
        {
            try
            {
                // Lấy thông tin người hủy
                var cancelledByUser = await _userCollection.Find(u => u.Id == cancelledByUserId).FirstOrDefaultAsync();
                var cancelledByName = cancelledByUser?.FullName ?? "Unknown";

                // Lấy thông tin cửa hàng
                var store = await _storeCollection.Find(s => s.SellerId == order.SellerId).FirstOrDefaultAsync();
                var storeName = store?.Name ?? "Unknown Store";

                string notificationMessage;
                string notificationUserId;

                if (cancelledByUserId == order.BuyerId)
                {
                    // Buyer hủy đơn -> thông báo cho seller
                    notificationUserId = order.SellerId;
                    notificationMessage = $"Đơn hàng #{order.Id} đã bị hủy bởi khách hàng {cancelledByName}. Lý do: {order.CancelReason}";
                }
                else
                {
                    // Seller hủy đơn -> thông báo cho buyer
                    notificationUserId = order.BuyerId;
                    notificationMessage = $"Đơn hàng #{order.Id} từ cửa hàng {storeName} đã bị hủy. Lý do: {order.CancelReason}";
                }

                var notification = new Notification
                {
                    UserId = notificationUserId,
                    Title = "Đơn hàng đã bị hủy",
                    Message = notificationMessage,
                    Type = "ORDER_CANCELLED",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepo.CreateAsync(notification);
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw để không ảnh hưởng đến việc hủy đơn hàng
                Console.Error.WriteLine($"Failed to create cancel notification: {ex.Message}");
            }
        }

        public async Task<bool> ConfirmOrderAsync(string orderId, string sellerId)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new Exception("Đơn hàng không tồn tại");
                }

                // Kiểm tra quyền (chỉ seller của đơn hàng)
                if (order.SellerId != sellerId)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xác nhận đơn hàng này");
                }

                // Kiểm tra trạng thái (chỉ có thể confirm khi Pending)
                if (order.Status != OrderStatus.Pending)
                {
                    throw new Exception($"Không thể xác nhận đơn hàng ở trạng thái {order.Status}");
                }

                // Cập nhật trạng thái
                order.Status = OrderStatus.Confirmed;
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepo.UpdateAsync(order.Id!, order);

                // Tạo notification cho buyer
                await CreateOrderStatusNotification(order, "Đơn hàng đã được xác nhận", 
                    "Người bán đã xác nhận còn hàng và sẵn sàng giao dịch.", "ORDER_CONFIRMED", true);

                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> MarkAsPaidAsync(string orderId, string sellerId)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new Exception("Đơn hàng không tồn tại");
                }

                // Kiểm tra quyền (chỉ seller của đơn hàng)
                if (order.SellerId != sellerId)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền cập nhật đơn hàng này");
                }

                // Kiểm tra trạng thái (chỉ có thể mark paid khi Confirmed)
                if (order.Status != OrderStatus.Confirmed)
                {
                    throw new Exception($"Không thể xác nhận thanh toán cho đơn hàng ở trạng thái {order.Status}");
                }

                // Cập nhật trạng thái
                order.Status = OrderStatus.Paid;
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepo.UpdateAsync(order.Id!, order);

                // Tạo notification cho buyer
                await CreateOrderStatusNotification(order, "Đã xác nhận thanh toán", 
                    "Người bán đã xác nhận nhận được tiền. Bạn có thể đến nhận hàng.", "PAYMENT_CONFIRMED", true);

                return true;
            }
            catch
            {
                throw;
            }
        }

        private async Task CreateOrderStatusNotification(Order order, string title, string message, string type, bool isBuyer = true)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = isBuyer ? order.BuyerId : order.SellerId, // Gửi cho buyer hoặc seller
                    Title = title,
                    Message = $"Đơn hàng #{order.Id}: {message}",
                    Type = type,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepo.CreateAsync(notification);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to create status notification: {ex.Message}");
            }
        }
    }
}