using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using LocalMartOnline.Repositories;
using AutoMapper;
using LocalMartOnline.Models.DTOs.Product;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Models.DTOs.ProxyShopping;
using LocalMartOnline.Models.DTOs.Seller;
namespace LocalMartOnline.Services.Implement
{
    public class ProxyShopperService : IProxyShopperService
    {
        private readonly IRepository<ProxyShoppingOrder> _orderRepo;
        private readonly IRepository<ProxyShopperRegistration> _proxyRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Store> _storeRepo;
        private readonly IRepository<ProxyRequest> _requestRepo;
        private readonly IRepository<ProductUnit> _productUnitRepo;
        private readonly IRepository<ProductImage> _productImageRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IMapper _mapper;

        public ProxyShopperService(
            IRepository<ProxyShoppingOrder> orderRepo,
            IRepository<ProxyShopperRegistration> proxyRepo,
            IRepository<User> userRepo,
            IRepository<Product> productRepo,
            IRepository<Store> storeRepo,
            IRepository<ProxyRequest> requestRepo,
            IRepository<ProductUnit> productUnitRepo,
            IRepository<ProductImage> productImageRepo,
            IRepository<Category> categoryRepo,
            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _proxyRepo = proxyRepo;
            _userRepo = userRepo;
            _productRepo = productRepo;
            _storeRepo = storeRepo;
            _requestRepo = requestRepo;
            _productUnitRepo = productUnitRepo;
            _productImageRepo = productImageRepo;
            _categoryRepo = categoryRepo;
            _mapper = mapper;
        }

        public async Task RegisterProxyShopperAsync(ProxyShopperRegistrationRequestDTO dto, string userId)
        {
            var registration = _mapper.Map<ProxyShopperRegistration>(dto);
            registration.UserId = userId!;
            registration.Status = "Pending";
            registration.CreatedAt = DateTime.Now;
            registration.UpdatedAt = DateTime.Now;
            await _proxyRepo.CreateAsync(registration);
        }

        public async Task<ProxyShopperRegistrationResponseDTO?> GetMyRegistrationAsync(string userId)
        {
            var myReg = await _proxyRepo.FindOneAsync(r => r.UserId == userId);
            if (myReg == null) return null;
            var dto = _mapper.Map<ProxyShopperRegistrationResponseDTO>(myReg);
            // Lấy thông tin user
            var user = await _userRepo.FindOneAsync(u => u.Id == userId);
            if (user != null)
            {
                dto.Name = user.FullName;
                dto.Email = user.Email;
                dto.PhoneNumber = user.PhoneNumber;
            }
            return dto;
        }

        public async Task<List<ProxyShopperRegistrationResponseDTO>> GetAllRegistrationsAsync()
        {
            var regs = await _proxyRepo.GetAllAsync();
            var userIds = regs.Select(r => r.UserId).Distinct().ToList();
            var users = await _userRepo.FindManyAsync(u => userIds.Contains(u.Id));
            var userDict = users.ToDictionary(u => u.Id, u => u);
            var result = new List<ProxyShopperRegistrationResponseDTO>();
            foreach (var reg in regs)
            {
                var dto = _mapper.Map<ProxyShopperRegistrationResponseDTO>(reg);
                if (userDict.TryGetValue(reg.UserId, out var user))
                {
                    dto.Name = user.FullName;
                    dto.Email = user.Email;
                    dto.PhoneNumber = user.PhoneNumber;
                }
                result.Add(dto);
            }
            return result;
        }

        public async Task<bool> ApproveRegistrationAsync(ProxyShopperRegistrationApproveDTO dto)
        {
            var reg = await _proxyRepo.FindOneAsync(r => r.Id == dto.RegistrationId);
            if (reg == null) return false;
            reg.Status = dto.Approve ? "Approved" : "Rejected";
            reg.RejectionReason = dto.Approve ? null : dto.RejectionReason;
            reg.UpdatedAt = DateTime.Now;
            await _proxyRepo.UpdateAsync(reg.Id!, reg);
            return true;
        }
        // 1. Buyer tạo request (yêu cầu đi chợ giùm)
        public async Task<string> CreateProxyRequestAsync(string buyerId, ProxyRequestDto proxyRequest)
        {
            if (proxyRequest == null || !proxyRequest.Items.Any())
                throw new ArgumentException("Danh sách sản phẩm không được để trống");
            var request = new ProxyRequest
            {
                BuyerId = buyerId,
                Items = proxyRequest.Items.Select(item => _mapper.Map<ProxyItem>(item)).ToList(),
                Status = ProxyRequestStatus.Open,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _requestRepo.CreateAsync(request);
            return request.Id;
        }

        // 2. Proxy xem các request còn trống (Open)
        public async Task<List<ProxyRequest>> GetAvailableRequestsAsync()
        {
            return (await _requestRepo.FindManyAsync(r => r.Status == ProxyRequestStatus.Open))
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public async Task<List<ProxyRequest>> GetMyAcceptedRequestsAsync(string proxyShopperId)
        {
            var myOrders = await _orderRepo.FindManyAsync(o => o.ProxyShopperId == proxyShopperId);
            var requestIds = myOrders.Select(o => o.ProxyRequestId).Where(id => !string.IsNullOrEmpty(id)).ToList();
            if (!requestIds.Any()) return new List<ProxyRequest>();
            var myRequests = await _requestRepo.FindManyAsync(r => requestIds.Contains(r.Id));
            return myRequests.ToList();
        }

        // 3. Proxy nhận request, atomic lock, tạo order (1-1)
        public async Task<string?> AcceptRequestAndCreateOrderAsync(string requestId, string proxyShopperId)
        {
            var req = await _requestRepo.FindOneAsync(r => r.Id == requestId);
            if (req == null || req.Status != ProxyRequestStatus.Open) return null;
            // Lock request
            req.Status = ProxyRequestStatus.Locked;
            req.UpdatedAt = DateTime.UtcNow;
            var ok = await _requestRepo.UpdateIfAsync(requestId, r => r.Status == ProxyRequestStatus.Open, req);
            if (!ok) return null;

            var order = new ProxyShoppingOrder
            {
                ProxyRequestId = requestId,
                BuyerId = req.BuyerId!,
                ProxyShopperId = proxyShopperId,
                Items = new List<ProductDto>(),
                TotalAmount = 0,
                ProxyFee = 0,
                Status = ProxyOrderStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _orderRepo.CreateAsync(order);

            Console.WriteLine($"[DEBUG] AcceptRequestAndCreateOrderAsync - Created Order with ID: {order.Id}");
            Console.WriteLine($"[DEBUG] AcceptRequestAndCreateOrderAsync - RequestId: {requestId}");

            req.ProxyShoppingOrderId = order.Id;
            req.UpdatedAt = DateTime.UtcNow;
            await _requestRepo.UpdateAsync(requestId, req);
            return order.Id;
        }

        // Advanced product search with weights
        public async Task<List<object>> AdvancedProductSearchAsync(string query, double wPrice, double wReputation, double wSold, double wStock)
        {
            var products = (await _productRepo.FindManyAsync(p => p.Name.ToLower().Contains(query.ToLower())))?.ToList() ?? new List<Product>();
            if (!products.Any()) return new List<object>();

            // Lấy thông tin các cửa hàng
            var storeIds = products.Where(p => !string.IsNullOrEmpty(p.StoreId)).Select(p => p.StoreId!).Distinct().ToList();
            var stores = storeIds.Any() ? await _storeRepo.FindManyAsync(s => storeIds.Contains(s.Id!)) : new List<Store>();
            var storeDict = stores.Where(s => s.Id != null).ToDictionary(s => s.Id!, s => s);

            // Lấy thông tin các đơn vị sản phẩm
            var unitIds = products.Where(p => !string.IsNullOrEmpty(p.UnitId)).Select(p => p.UnitId!).Distinct().ToList();
            var units = unitIds.Any() ? await _productUnitRepo.FindManyAsync(u => unitIds.Contains(u.Id!)) : new List<ProductUnit>();
            var unitDict = units.Where(u => u.Id != null).ToDictionary(u => u.Id!, u => u);

            // Lấy thông tin hình ảnh sản phẩm
            var productIds = products.Select(p => p.Id!).ToList();
            var productImages = productIds.Any() ? await _productImageRepo.FindManyAsync(img => productIds.Contains(img.ProductId)) : new List<ProductImage>();
            var imageDict = productImages.GroupBy(img => img.ProductId).ToDictionary(g => g.Key, g => g.Select(img => img.ImageUrl).ToList());

            // Chuẩn hóa các giá trị dựa trên các trường có sẵn
            double minPrice = products.Min(p => (double)p.Price);
            double maxPrice = products.Max(p => (double)p.Price);
            // Sử dụng rating trung bình từ store thay vì SellerReputation
            double minRep = 0; // Tạm thời đặt = 0 vì chưa có trường reputation
            double maxRep = 5; // Giả định thang điểm 0-5
            double minSold = products.Min(p => (double)p.PurchaseCount);
            double maxSold = products.Max(p => (double)p.PurchaseCount);

            var result = products.Select(p =>
            {
                double priceScore = (maxPrice - minPrice) > 0 ? 1 - ((double)p.Price - minPrice) / (maxPrice - minPrice) : 1;
                // Sử dụng giá trị cố định cho reputation score tạm thời
                double reputationScore = 0.5; // Giá trị trung bình tạm thời
                double soldScore = (maxSold - minSold) > 0 ? ((double)p.PurchaseCount - minSold) / (maxSold - minSold) : 1;
                // Sử dụng ProductStatus thay vì InStock
                double stockScore = p.Status == ProductStatus.Active ? 1 : 0;
                double score = wPrice * priceScore + wReputation * reputationScore + wSold * soldScore + wStock * stockScore;
                
                string? storeName = null;
                if (!string.IsNullOrEmpty(p.StoreId) && storeDict.TryGetValue(p.StoreId, out var store))
                {
                    storeName = store.Name;
                }

                // Lấy thông tin đơn vị
                string? unitName = null;
                if (!string.IsNullOrEmpty(p.UnitId) && unitDict.TryGetValue(p.UnitId, out var unit))
                {
                    unitName = unit.DisplayName;
                }

                // Lấy danh sách hình ảnh
                var images = new List<string>();
                if (!string.IsNullOrEmpty(p.Id) && imageDict.TryGetValue(p.Id, out var productImages))
                {
                    images = productImages;
                }

                return new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    SellerReputation = reputationScore * 5, // Chuyển về thang điểm 0-5
                    p.PurchaseCount,
                    InStock = p.Status == ProductStatus.Active,
                    StoreName = storeName,
                    UnitName = unitName,
                    Images = images,
                    p.Status,
                    Score = score
                };
            })
            .OrderByDescending(x => x.Score)
            .Cast<object>()
            .ToList();
            return result;
        }

        // 4. Proxy lên đơn, gửi đề xuất (điền sản phẩm thật + phí)
        public async Task<bool> SendProposalAsync(string requestId, ProxyShoppingProposalDTO proposal)
        {
            try
            {
                Console.WriteLine($"[DEBUG] SendProposalAsync - Starting with RequestId: {requestId}");
                
                // Tìm order từ requestId
                var order = await _orderRepo.FindOneAsync(o => o.ProxyRequestId == requestId);
                if (order == null)
                {
                    Console.WriteLine($"[DEBUG] SendProposalAsync - Order not found for RequestId: {requestId}");
                    return false;
                }

                Console.WriteLine($"[DEBUG] SendProposalAsync - Order found. OrderId: {order.Id}, Status: {order.Status}");
                Console.WriteLine($"[DEBUG] SendProposalAsync - Order BuyerId: {order.BuyerId}");
                Console.WriteLine($"[DEBUG] SendProposalAsync - Order ProxyShopperId: {order.ProxyShopperId}");

                // Kiểm tra order phải ở trạng thái Draft
                if (order.Status != ProxyOrderStatus.Draft)
                {
                    Console.WriteLine($"[DEBUG] SendProposalAsync - Order not in Draft status. Current status: {order.Status}");
                    return false;
                }

                // Chuyển đổi từ ProxyShoppingProposalItemDto sang ProductDto
                Console.WriteLine($"[DEBUG] SendProposalAsync - Converting {proposal.Items.Count} items");
                var productDtos = proposal.Items.Select((item, index) => 
                {
                    Console.WriteLine($"[DEBUG] SendProposalAsync - Item {index + 1}: Id={item.Id}, Name={item.Name}, Quantity={item.Quantity}, Unit={item.Unit}, Price={item.Price}");
                    return new ProductDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Price = item.Price,
                        UnitName = item.Unit,
                        MinimumQuantity = item.Quantity
                    };
                }).ToList();

                Console.WriteLine($"[DEBUG] SendProposalAsync - Updating order with {productDtos.Count} items");
                order.Items = productDtos;
                order.TotalAmount = proposal.TotalAmount;
                order.ProxyFee = proposal.ProxyFee;
                order.Notes = proposal.Note;
                order.Status = ProxyOrderStatus.Proposed;
                order.UpdatedAt = DateTime.UtcNow;

                Console.WriteLine($"[DEBUG] SendProposalAsync - Order before update:");
                Console.WriteLine($"[DEBUG] SendProposalAsync - TotalAmount: {order.TotalAmount}");
                Console.WriteLine($"[DEBUG] SendProposalAsync - ProxyFee: {order.ProxyFee}");
                Console.WriteLine($"[DEBUG] SendProposalAsync - Notes: {order.Notes}");
                Console.WriteLine($"[DEBUG] SendProposalAsync - Status: {order.Status}");

                await _orderRepo.UpdateAsync(order.Id!, order);
                Console.WriteLine($"[DEBUG] SendProposalAsync - Order updated successfully");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendProposalAsync - Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] SendProposalAsync - StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        // 5. Buyer duyệt & thanh toán
        public async Task<bool> BuyerApproveAndPayAsync(string orderId, string buyerId)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId && o.BuyerId == buyerId);
            if (order == null || order.Status != ProxyOrderStatus.Proposed) return false;
            // Thực hiện thanh toán ở đây (TODO)
            order.Status = ProxyOrderStatus.Paid;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(orderId, order);
            return true;
        }

        // 6. Proxy bắt đầu mua hàng (chuyển trạng thái)
        public async Task<bool> StartShoppingAsync(string orderId, string proxyShopperId)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId && o.ProxyShopperId == proxyShopperId);
            if (order == null || order.Status != ProxyOrderStatus.Paid) return false;
            order.Status = ProxyOrderStatus.InProgress;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(orderId, order);
            return true;
        }

        // 7. Proxy upload ảnh hàng hóa, ghi chú...
        public async Task<bool> UploadBoughtItemsAsync(string orderId, List<string> imageUrls, string? note)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId);
            if (order == null || order.Status != ProxyOrderStatus.InProgress) return false;
            // TODO: lưu imageUrls vào field mới (ProofImages)
            order.Notes = note;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(orderId, order);
            return true;
        }

        // 8. Buyer xác nhận nhận hàng (hoàn tất)
        public async Task<bool> ConfirmDeliveryAsync(string orderId, string buyerId)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId && o.BuyerId == buyerId);
            if (order == null || order.Status != ProxyOrderStatus.InProgress) return false;
            order.Status = ProxyOrderStatus.Completed;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(orderId, order);

            // Đóng request
            var req = await _requestRepo.FindOneAsync(r => r.Id == order.ProxyRequestId);
            if (req != null)
            {
                req.Status = ProxyRequestStatus.Completed;
                req.UpdatedAt = DateTime.UtcNow;
                await _requestRepo.UpdateAsync(req.Id!, req);
            }

            // Update product purchase count
            if (order.Items != null)
            {
                foreach (var item in order.Items)
                {
                    if (string.IsNullOrEmpty(item.Id)) continue;
                    var product = await _productRepo.FindOneAsync(p => p.Id == item.Id);
                    if (product != null)
                    {
                        product.PurchaseCount += 1; // Tăng 1 lần mua (không phụ thuộc số lượng)
                        await _productRepo.UpdateAsync(product.Id!, product);
                    }
                }
            }
            return true;
        }

        // 9. Hủy đơn – mở lại request (nếu chưa mua hàng)
        public async Task<bool> CancelOrderAsync(string orderId, string proxyShopperId, string reason)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId && o.ProxyShopperId == proxyShopperId);
            if (order == null || order.Status is ProxyOrderStatus.Completed or ProxyOrderStatus.Cancelled) return false;

            order.Status = ProxyOrderStatus.Cancelled;
            order.Notes = $"Hủy bởi ProxyShopper: {reason}";
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(orderId, order);

            // reopen request nếu order còn ở giai đoạn Draft/Proposed/Paid
            var req = await _requestRepo.FindOneAsync(r => r.Id == order.ProxyRequestId);
            if (req != null && req.Status == ProxyRequestStatus.Locked)
            {
                req.Status = ProxyRequestStatus.Open;
                req.ProxyShoppingOrderId = null;
                req.UpdatedAt = DateTime.UtcNow;
                await _requestRepo.UpdateAsync(req.Id!, req);
            }
            return true;
        }

        // Lấy thông tin chi tiết của một request theo ID
        public async Task<ProxyRequestResponseDto?> GetRequestByIdAsync(string requestId)
        {
            var request = await _requestRepo.FindOneAsync(r => r.Id == requestId);
            if (request == null) return null;

            // Lấy thông tin buyer
            var buyer = await _userRepo.FindOneAsync(u => u.Id == request.BuyerId);
            
            return new ProxyRequestResponseDto
            {
                Id = request.Id,
                Items = request.Items,
                Status = request.Status.ToString(),
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                BuyerName = buyer?.FullName,
                BuyerEmail = buyer?.Email,
                BuyerPhone = buyer?.PhoneNumber
            };
        }

        // Lấy OrderId từ RequestId
        public async Task<string?> GetOrderIdByRequestIdAsync(string requestId)
        {
            var order = await _orderRepo.FindOneAsync(o => o.ProxyRequestId == requestId);
            return order?.Id;
        }

        // Lấy danh sách yêu cầu của buyer với thông tin đề xuất
        public async Task<List<BuyerRequestWithProposalDto>> GetMyRequestsWithProposalsAsync(string buyerId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] GetMyRequestsWithProposals - BuyerId: {buyerId}");
                
                // Lấy tất cả requests của buyer
                var requests = await _requestRepo.FindManyAsync(r => r.BuyerId == buyerId);
                var result = new List<BuyerRequestWithProposalDto>();

                foreach (var request in requests)
                {
                    var dto = new BuyerRequestWithProposalDto
                    {
                        Id = request.Id!,
                        Items = request.Items,
                        Status = request.Status.ToString(),
                        CreatedAt = request.CreatedAt,
                        UpdatedAt = request.UpdatedAt
                    };

                    // Tìm order tương ứng (nếu có)
                    var order = await _orderRepo.FindOneAsync(o => o.ProxyRequestId == request.Id);
                    if (order != null)
                    {
                        dto.ProxyShopperId = order.ProxyShopperId;

                        // Lấy thông tin proxy shopper
                        if (!string.IsNullOrEmpty(order.ProxyShopperId))
                        {
                            var proxyUser = await _userRepo.FindOneAsync(u => u.Id == order.ProxyShopperId);
                            if (proxyUser != null)
                            {
                                dto.ProxyShopperName = proxyUser.FullName;
                                dto.ProxyShopperPhone = proxyUser.PhoneNumber;
                            }
                        }

                        // Nếu có đề xuất (order status != Draft)
                        if (order.Status != ProxyOrderStatus.Draft && order.Items != null && order.Items.Any())
                        {
                            // Chuyển đổi sang ProductStorePageDto với thông tin chi tiết
                            var detailedItems = await ConvertToProductStorePageDto(order.Items);
                            
                            dto.Proposal = new ProposalInfo
                            {
                                ProposedItems = detailedItems,
                                TotalAmount = order.TotalAmount,
                                ProxyFee = order.ProxyFee,
                                Notes = order.Notes,
                                ProposedAt = order.UpdatedAt,
                                OrderStatus = order.Status.ToString()
                            };
                        }
                    }

                    result.Add(dto);
                }

                Console.WriteLine($"[DEBUG] GetMyRequestsWithProposals - Found {result.Count} requests");
                return result.OrderByDescending(r => r.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetMyRequestsWithProposals - Exception: {ex.Message}");
                return new List<BuyerRequestWithProposalDto>();
            }
        }

        // Helper method để chuyển đổi ProductDto sang ProductStorePageDto với thông tin chi tiết
        private async Task<List<ProductStorePageDto>> ConvertToProductStorePageDto(List<ProductDto> items)
        {
            var result = new List<ProductStorePageDto>();
            
            foreach (var item in items)
            {
                try
                {
                    // Lấy thông tin sản phẩm chi tiết
                    var product = await _productRepo.FindOneAsync(p => p.Id == item.Id);
                    if (product == null) continue;
                    
                    // Lấy thông tin cửa hàng
                    var store = !string.IsNullOrEmpty(product.StoreId) 
                        ? await _storeRepo.FindOneAsync(s => s.Id == product.StoreId) 
                        : null;
                    
                    // Lấy hình ảnh sản phẩm
                    var images = await _productImageRepo.FindManyAsync(img => img.ProductId == product.Id);
                    
                    // Lấy tên category
                    var category = !string.IsNullOrEmpty(product.CategoryId)
                        ? await _categoryRepo.FindOneAsync(c => c.Id == product.CategoryId)
                        : null;
                    
                    var storePageDto = new ProductStorePageDto
                    {
                        Id = item.Id ?? "",
                        Name = item.Name,
                        Price = item.Price,
                        Quantity = (int)item.MinimumQuantity,
                        Unit = item.UnitName ?? "",
                        
                        // Thông tin cửa hàng
                        StoreId = product.StoreId ?? "",
                        StoreName = store?.Name ?? "N/A",
                        StoreAddress = store?.Address ?? "N/A",
                        StoreRating = (double)(store?.Rating ?? 0),
                        StoreTotalOrders = 0, // Store model không có field này, tạm để 0
                        
                        // Thông tin sản phẩm
                        Description = product.Description ?? "",
                        PurchaseCount = product.PurchaseCount,
                        StockQuantity = 100, // Product model không có field này, tạm để 100
                        ProductRating = 0, // Product model không có field này, tạm để 0
                        ImageUrls = images.Select(img => img.ImageUrl).ToList(),
                        
                        // Thông tin bổ sung
                        CategoryName = category?.Name ?? "",
                        IsAvailable = product.Status == ProductStatus.Active,
                        UpdatedAt = product.UpdatedAt
                    };
                    
                    result.Add(storePageDto);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] ConvertToProductStorePageDto - Item {item.Id}: {ex.Message}");
                    // Thêm item cơ bản nếu không lấy được thông tin chi tiết
                    result.Add(new ProductStorePageDto
                    {
                        Id = item.Id ?? "",
                        Name = item.Name,
                        Price = item.Price,
                        Quantity = (int)item.MinimumQuantity,
                        Unit = item.UnitName ?? "",
                        StoreName = "N/A",
                        IsAvailable = false
                    });
                }
            }
            
            return result;
        }
    }
}
