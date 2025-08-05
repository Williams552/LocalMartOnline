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
using LocalMartOnline.Services;
namespace LocalMartOnline.Services.Implement
{
    public class ProxyShopperService : IProxyShopperService
    {
        private readonly IRepository<ProxyShoppingOrder> _orderRepo;
        private readonly IRepository<ProxyShopperRegistration> _proxyRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Store> _storeRepo;
        private readonly IRepository<Market> _marketRepo;
        private readonly IRepository<ProxyRequest> _requestRepo;
        private readonly IRepository<ProductUnit> _productUnitRepo;
        private readonly IRepository<ProductImage> _productImageRepo;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        // ADMIN: Lấy danh sách tất cả proxy requests
        public async Task<List<AdminProxyRequestDto>> GetAllProxyRequestsAsync()
        {
            var requests = await _requestRepo.GetAllAsync();
            var userIds = requests.Select(r => r.BuyerId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            var proxyIds = requests.Where(r => r.ProxyShoppingOrderId != null)
                .Select(r => r.ProxyShoppingOrderId)
                .Distinct()
                .ToList();
            var users = await _userRepo.FindManyAsync(u => u.Id != null && userIds.Contains(u.Id));
            var userDict = users.Where(u => u.Id != null).ToDictionary(u => u.Id!, u => u);
            var orders = await _orderRepo.GetAllAsync();
            var orderDict = orders.Where(o => o.Id != null).ToDictionary(o => o.Id!, o => o);
            var result = new List<AdminProxyRequestDto>();
            foreach (var req in requests)
            {
                var dto = new AdminProxyRequestDto
                {
                    Id = req.Id,
                    ProxyOrderId = req.ProxyShoppingOrderId,
                    BuyerId = req.BuyerId,
                    BuyerName = userDict.TryGetValue(req.BuyerId, out var buyer) ? buyer.FullName : null,
                    BuyerEmail = userDict.TryGetValue(req.BuyerId, out var buyer2) ? buyer2.Email : null,
                    BuyerPhone = userDict.TryGetValue(req.BuyerId, out var buyer3) ? buyer3.PhoneNumber : null,
                    Status = req.Status.ToString(),
                    RequestStatus = req.Status.ToString(),
                    CreatedAt = req.CreatedAt,
                    Items = req.Items,
                };
                if (!string.IsNullOrEmpty(req.ProxyShoppingOrderId) && orderDict.TryGetValue(req.ProxyShoppingOrderId, out var order))
                {
                    dto.ProxyShopperId = order.ProxyShopperId;
                    var proxyUser = await _userRepo.FindOneAsync(u => u.Id == order.ProxyShopperId);
                    dto.ProxyShopperName = proxyUser?.FullName;
                    dto.ProxyShopperEmail = proxyUser?.Email;
                    dto.ProxyShopperPhone = proxyUser?.PhoneNumber;
                    dto.OrderStatus = order.Status.ToString();
                    dto.TotalAmount = order.TotalAmount;
                }
                result.Add(dto);
            }
            return result;
        }

        // ADMIN: Lấy chi tiết proxy request theo id
        public async Task<ProxyRequestsResponseDto?> GetProxyRequestDetailForAdminAsync(string requestId)
        {
            var req = await _requestRepo.FindOneAsync(r => r.Id == requestId);
            if (req == null) return null;
            var buyer = await _userRepo.FindOneAsync(u => u.Id == req.BuyerId);
            ProxyShoppingOrder? order = null;
            if (!string.IsNullOrEmpty(req.ProxyShoppingOrderId))
                order = await _orderRepo.FindOneAsync(o => o.Id == req.ProxyShoppingOrderId);
            var dto = new ProxyRequestsResponseDto
            {
                Id = req.Id,
                ProxyOrderId = req.ProxyShoppingOrderId,
                Items = req.Items,
                Status = req.Status.ToString(),
                CreatedAt = req.CreatedAt,
                UpdatedAt = req.UpdatedAt,
                PartnerName = buyer?.FullName,
                PartnerEmail = buyer?.Email,
                PartnerPhone = buyer?.PhoneNumber,
                PartnerRole = "Buyer",
                // Thông tin người mua
                BuyerName = buyer?.FullName,
                BuyerEmail = buyer?.Email,
                BuyerPhone = buyer?.PhoneNumber,
            };
            if (order != null)
            {
                var proxyUser = await _userRepo.FindOneAsync(u => u.Id == order.ProxyShopperId);
                dto.OrderId = order.Id;
                dto.OrderStatus = order.Status.ToString();
                dto.OrderItems = order.Items;
                dto.TotalAmount = order.TotalAmount;
                dto.ProxyFee = order.ProxyFee;
                dto.DeliveryAddress = order.DeliveryAddress;
                dto.Notes = order.Notes;
                dto.ProofImages = order.ProofImages;
                dto.OrderCreatedAt = order.CreatedAt;
                dto.OrderUpdatedAt = order.UpdatedAt;
                dto.PartnerName = proxyUser?.FullName;
                dto.PartnerEmail = proxyUser?.Email;
                dto.PartnerPhone = proxyUser?.PhoneNumber;
                dto.PartnerRole = "Proxy Shopper";
            }
            return dto;
        }

        // ADMIN: Cập nhật trạng thái proxy request
        public async Task<bool> UpdateProxyRequestStatusAsync(string requestId, string status)
        {
            var req = await _requestRepo.FindOneAsync(r => r.Id == requestId);
            if (req == null) return false;
            req.Status = Enum.TryParse<ProxyRequestStatus>(status, out var s) ? s : req.Status;
            req.UpdatedAt = DateTime.UtcNow;
            await _requestRepo.UpdateAsync(requestId, req);
            return true;
        }

        // ADMIN: Cập nhật trạng thái proxy order
        public async Task<bool> UpdateProxyOrderStatusAsync(string orderId, string status)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId);
            if (order == null) return false;
            order.Status = Enum.TryParse<ProxyOrderStatus>(status, out var s) ? s : order.Status;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(orderId, order);
            return true;
        }
        public ProxyShopperService(
            IRepository<ProxyShoppingOrder> orderRepo,
            IRepository<ProxyShopperRegistration> proxyRepo,
            IRepository<User> userRepo,
            IRepository<Product> productRepo,
            IRepository<Store> storeRepo,
            IRepository<Market> marketRepo,
            IRepository<ProxyRequest> requestRepo,
            IRepository<ProductUnit> productUnitRepo,
            IRepository<ProductImage> productImageRepo,
            IMapper mapper,
            INotificationService notificationService)
        {
            _orderRepo = orderRepo;
            _proxyRepo = proxyRepo;
            _userRepo = userRepo;
            _productRepo = productRepo;
            _storeRepo = storeRepo;
            _marketRepo = marketRepo;
            _requestRepo = requestRepo;
            _productUnitRepo = productUnitRepo;
            _productImageRepo = productImageRepo;
            _mapper = mapper;
            _notificationService = notificationService;
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
            var userIds = regs.Select(r => r.UserId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            var users = await _userRepo.FindManyAsync(u => u.Id != null && userIds.Contains(u.Id));
            var userDict = users.Where(u => u.Id != null).ToDictionary(u => u.Id!, u => u);
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

            // Nếu approve thành công, cập nhật role của user thành "Proxy Shopper"
            if (dto.Approve)
            {
                var user = await _userRepo.FindOneAsync(u => u.Id == reg.UserId);
                if (user != null)
                {
                    user.Role = "Proxy Shopper";
                    await _userRepo.UpdateAsync(user.Id!, user);
                    Console.WriteLine($"[INFO] ApproveRegistrationAsync - Updated user {user.Id} role to 'Proxy Shopper'");
                }
            }
            
            return true;
        }
        // 1. Buyer tạo request (yêu cầu đi chợ giùm)
        public async Task<string> CreateProxyRequestAsync(string buyerId, ProxyRequestDto proxyRequest)
        {
            if (proxyRequest == null || !proxyRequest.Items.Any())
                throw new ArgumentException("Danh sách sản phẩm không được để trống");
            if (string.IsNullOrEmpty(proxyRequest.MarketId))
                throw new ArgumentException("Bạn phải chọn chợ trước khi tạo yêu cầu đi chợ giùm.");

            var request = new ProxyRequest
            {
                BuyerId = buyerId,
                MarketId = proxyRequest.MarketId,
                Items = proxyRequest.Items.Select(item => _mapper.Map<ProxyItem>(item)).ToList(),
                Status = ProxyRequestStatus.Open,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _requestRepo.CreateAsync(request);
            return request.Id;
        }

        // 2. Proxy xem các request còn trống (Open) trong chợ đã đăng ký
        public async Task<List<ProxyRequest>> GetAvailableRequestsAsync()
        {
            return (await _requestRepo.FindManyAsync(r => r.Status == ProxyRequestStatus.Open))
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        // 2a. Proxy xem các request còn trống (Open) trong chợ đã đăng ký
        public async Task<List<ProxyRequest>> GetAvailableRequestsForProxyAsync(string proxyShopperId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] GetAvailableRequestsForProxyAsync - Starting for ProxyShopperId: {proxyShopperId}");
                
                // Lấy thông tin proxy shopper registration để biết MarketId
                var proxyRegistration = await _proxyRepo.FindOneAsync(p => p.UserId == proxyShopperId && p.Status == "Approved");
                if (proxyRegistration == null)
                {
                    Console.WriteLine($"[DEBUG] GetAvailableRequestsForProxyAsync - No approved registration found for proxy: {proxyShopperId}");
                    return new List<ProxyRequest>();
                }

                Console.WriteLine($"[DEBUG] GetAvailableRequestsForProxyAsync - Proxy registered for MarketId: {proxyRegistration.MarketId}");

                // Lấy các request Open trong chợ mà proxy đã đăng ký
                var availableRequests = await _requestRepo.FindManyAsync(r => 
                    r.Status == ProxyRequestStatus.Open && 
                    r.MarketId == proxyRegistration.MarketId);

                var result = availableRequests.OrderByDescending(r => r.CreatedAt).ToList();
                Console.WriteLine($"[DEBUG] GetAvailableRequestsForProxyAsync - Found {result.Count} available requests");
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetAvailableRequestsForProxyAsync - Exception: {ex.Message}");
                return new List<ProxyRequest>();
            }
        }

        public async Task<List<ProxyShopperAcceptedRequestDto>> GetMyAcceptedRequestsAsync(string proxyShopperId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] GetMyAcceptedRequestsAsync - Starting for ProxyShopperId: {proxyShopperId}");
                
                // Lấy tất cả orders của proxy shopper
                var myOrders = await _orderRepo.FindManyAsync(o => o.ProxyShopperId == proxyShopperId);
                Console.WriteLine($"[DEBUG] GetMyAcceptedRequestsAsync - Found {myOrders.Count()} orders");

                if (!myOrders.Any()) 
                {
                    Console.WriteLine($"[DEBUG] GetMyAcceptedRequestsAsync - No orders found");
                    return new List<ProxyShopperAcceptedRequestDto>();
                }

                // Lấy tất cả request IDs từ orders
                var requestIds = myOrders.Select(o => o.ProxyRequestId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
                Console.WriteLine($"[DEBUG] GetMyAcceptedRequestsAsync - Found {requestIds.Count} unique request IDs");

                if (!requestIds.Any()) 
                {
                    Console.WriteLine($"[DEBUG] GetMyAcceptedRequestsAsync - No valid request IDs found");
                    return new List<ProxyShopperAcceptedRequestDto>();
                }

                // Lấy tất cả requests
                var myRequests = await _requestRepo.FindManyAsync(r => requestIds.Contains(r.Id));
                Console.WriteLine($"[DEBUG] GetMyAcceptedRequestsAsync - Found {myRequests.Count()} requests");

                // Lấy thông tin buyers
                var buyerIds = myRequests.Select(r => r.BuyerId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
                var users = await _userRepo.FindManyAsync(u => u.Id != null && buyerIds.Contains(u.Id));
                var userDict = users.Where(u => u.Id != null).ToDictionary(u => u.Id!, u => u);
                Console.WriteLine($"[DEBUG] GetMyAcceptedRequestsAsync - Found {users.Count()} users");

                // Tạo dictionary để map request -> order
                var orderDict = myOrders.Where(o => !string.IsNullOrEmpty(o.ProxyRequestId))
                                       .ToDictionary(o => o.ProxyRequestId!, o => o);

                var result = new List<ProxyShopperAcceptedRequestDto>();

                foreach (var request in myRequests)
                {
                    Console.WriteLine($"[DEBUG] GetMyAcceptedRequestsAsync - Processing request: {request.Id}");
                    
                    // Lấy thông tin buyer
                    var buyer = userDict.TryGetValue(request.BuyerId, out var user) ? user : null;
                    
                    // Lấy thông tin order tương ứng
                    var order = orderDict.TryGetValue(request.Id, out var ord) ? ord : null;
                    
                    // Tính toán current phase và permissions
                    var (currentPhase, canEditProposal, canStartShopping, canUploadProof, canCancel) = CalculateOrderPhaseAndPermissions(request, order);

                    var dto = new ProxyShopperAcceptedRequestDto
                    {
                        // Request Information
                        RequestId = request.Id,
                        RequestItems = request.Items ?? new List<ProxyItem>(),
                        RequestStatus = request.Status.ToString(),
                        RequestCreatedAt = request.CreatedAt,
                        RequestUpdatedAt = request.UpdatedAt,
                        
                        // Buyer Information
                        BuyerName = buyer?.FullName,
                        BuyerEmail = buyer?.Email,
                        BuyerPhone = buyer?.PhoneNumber,
                        
                        // Order Information
                        OrderId = order?.Id,
                        OrderStatus = order?.Status.ToString(),
                        OrderItems = order?.Items ?? new List<ProductDto>(),
                        TotalAmount = order?.TotalAmount,
                        ProxyFee = order?.ProxyFee,
                        DeliveryAddress = order?.DeliveryAddress,
                        Notes = order?.Notes,
                        ProofImages = order?.ProofImages,
                        OrderCreatedAt = order?.CreatedAt,
                        OrderUpdatedAt = order?.UpdatedAt,
                        
                        // UI State
                        CurrentPhase = currentPhase,
                        CanEditProposal = canEditProposal,
                        CanStartShopping = canStartShopping,
                        CanUploadProof = canUploadProof,
                        CanCancel = canCancel
                    };

                    result.Add(dto);
                    Console.WriteLine($"[DEBUG] GetMyAcceptedRequestsAsync - Added DTO for request {request.Id} with phase {currentPhase}");
                }

                // Sắp xếp theo thời gian tạo request (mới nhất trước)
                result = result.OrderByDescending(r => r.RequestCreatedAt).ToList();
                
                Console.WriteLine($"[DEBUG] GetMyAcceptedRequestsAsync - Returning {result.Count} results");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetMyAcceptedRequestsAsync - Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] GetMyAcceptedRequestsAsync - StackTrace: {ex.StackTrace}");
                return new List<ProxyShopperAcceptedRequestDto>();
            }
        }

        public async Task<List<ProxyRequestsResponseDto>> GetMyRequestsAsync(string userId, string userRole)
        {
            try
            {
                List<ProxyRequest> myRequests;
                List<ProxyShoppingOrder> relatedOrders;

                if (userRole == "Buyer")
                {
                    // Buyer: Lấy các request mà họ đã tạo
                    myRequests = (await _requestRepo.FindManyAsync(r => r.BuyerId == userId)).ToList();
                    // Lấy các order tương ứng với requests của buyer
                    var requestIds = myRequests.Select(r => r.Id).ToList();
                    relatedOrders = requestIds.Any() 
                        ? (await _orderRepo.FindManyAsync(o => o.ProxyRequestId != null && requestIds.Contains(o.ProxyRequestId))).ToList()
                        : new List<ProxyShoppingOrder>();
                }
                else // Proxy Shopper
                {
                    // Proxy Shopper: Lấy các order mà họ đã nhận, rồi lấy request tương ứng
                    relatedOrders = (await _orderRepo.FindManyAsync(o => o.ProxyShopperId == userId)).ToList();

                    var requestIds = relatedOrders.Where(o => !string.IsNullOrEmpty(o.ProxyRequestId))
                                                  .Select(o => o.ProxyRequestId!)
                                                  .Distinct()
                                                  .ToList();
                    
                    myRequests = requestIds.Any() 
                        ? (await _requestRepo.FindManyAsync(r => r.Id != null && requestIds.Contains(r.Id))).ToList()
                        : new List<ProxyRequest>();
                }
                if (!myRequests.Any())
                {
                    return new List<ProxyRequestsResponseDto>();
                }

                // Lấy thông tin partners (đối tác)
                var partnerIds = new List<string>();
                if (userRole == "Buyer")
                {
                    // Lấy danh sách ProxyShopperId từ orders
                    partnerIds = relatedOrders.Where(o => !string.IsNullOrEmpty(o.ProxyShopperId))
                                              .Select(o => o.ProxyShopperId!)
                                              .Distinct()
                                              .ToList();
                }
                else
                {
                    // Lấy danh sách BuyerId từ requests
                    partnerIds = myRequests.Select(r => r.BuyerId).Distinct().ToList();
                }

                var partners = partnerIds.Any() 
                    ? await _userRepo.FindManyAsync(u => u.Id != null && partnerIds.Contains(u.Id))
                    : new List<User>();
                var partnerDict = partners.Where(u => u.Id != null).ToDictionary(u => u.Id!, u => u);

                // Lấy thông tin stores
                var storeIds = myRequests.Select(r => r.MarketId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
                var stores = storeIds.Any() 
                    ? await _storeRepo.FindManyAsync(s => s.Id != null && storeIds.Contains(s.Id))
                    : new List<Store>();
                var storeDict = stores.Where(s => s.Id != null).ToDictionary(s => s.Id!, s => s);

                // Tạo dictionary để map request -> order
                var orderDict = relatedOrders.Where(o => !string.IsNullOrEmpty(o.ProxyRequestId))
                                             .ToDictionary(o => o.ProxyRequestId!, o => o);

                var result = new List<ProxyRequestsResponseDto>();

                foreach (var request in myRequests)
                {
                    // Lấy thông tin order tương ứng
                    var order = orderDict.TryGetValue(request.Id, out var ord) ? ord : null;

                    // Lấy thông tin partner
                    string? partnerName = null;
                    string? partnerEmail = null;
                    string? partnerPhone = null;
                    string partnerRole = "Unknown";

                    if (userRole == "Buyer" && order != null && !string.IsNullOrEmpty(order.ProxyShopperId))
                    {
                        // Buyer xem thông tin Proxy Shopper
                        if (partnerDict.TryGetValue(order.ProxyShopperId, out var proxy))
                        {
                            partnerName = proxy.FullName;
                            partnerEmail = proxy.Email;
                            partnerPhone = proxy.PhoneNumber;
                            partnerRole = "Proxy Shopper";
                        }
                    }
                    else if (userRole == "Proxy Shopper")
                    {
                        // Proxy Shopper xem thông tin Buyer
                        if (partnerDict.TryGetValue(request.BuyerId, out var buyer))
                        {
                            partnerName = buyer.FullName;
                            partnerEmail = buyer.Email;
                            partnerPhone = buyer.PhoneNumber;
                            partnerRole = "Buyer";
                        }
                    }

                    // Tính toán current phase
                    string currentPhase = "Chưa có Proxy nhận";
                    if (order != null)
                    {
                        currentPhase = order.Status switch
                        {
                            ProxyOrderStatus.Draft => "Đang soạn đơn",
                            ProxyOrderStatus.Proposed => "Chờ duyệt",
                            ProxyOrderStatus.Paid => "Đã thanh toán",
                            ProxyOrderStatus.InProgress => "Đang mua hàng",
                            ProxyOrderStatus.Completed => "Đã hoàn thành",
                            ProxyOrderStatus.Cancelled => "Đã hủy",
                            ProxyOrderStatus.Expired => "Đã hết hạn",
                            _ => "Không xác định"
                        };
                    }

                    var dto = new ProxyRequestsResponseDto
                    {
                        // Request Information
                        Id = request.Id,
                        Items = request.Items ?? new List<ProxyItem>(),
                        Status = request.Status.ToString(),
                        CreatedAt = request.CreatedAt,
                        UpdatedAt = request.UpdatedAt,
                        StoreId = request.MarketId,
                        StoreName = !string.IsNullOrEmpty(request.MarketId) && storeDict.TryGetValue(request.MarketId, out var store) ? store.Name : null,
                        
                        // Partner Information
                        PartnerName = partnerName,
                        PartnerEmail = partnerEmail,
                        PartnerPhone = partnerPhone,
                        PartnerRole = partnerRole,
                        
                        // Order Information
                        OrderId = order?.Id,
                        OrderStatus = order?.Status.ToString(),
                        OrderItems = order?.Items,
                        TotalAmount = order?.TotalAmount,
                        ProxyFee = order?.ProxyFee,
                        DeliveryAddress = order?.DeliveryAddress,
                        Notes = order?.Notes,
                        ProofImages = order?.ProofImages,
                        OrderCreatedAt = order?.CreatedAt,
                        OrderUpdatedAt = order?.UpdatedAt,
                        
                        // UI Helpers
                        CurrentPhase = currentPhase
                    };

                    result.Add(dto);
                }

                return result.OrderByDescending(r => r.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                return new List<ProxyRequestsResponseDto>();
            }
        }

        private (string currentPhase, bool canEditProposal, bool canStartShopping, bool canUploadProof, bool canCancel) 
            CalculateOrderPhaseAndPermissions(ProxyRequest request, ProxyShoppingOrder? order)
        {
            if (order == null)
            {
                // Chỉ có request, chưa có order (không bao giờ xảy ra trong flow này, nhưng để đảm bảo)
                return ("Request Only", false, false, false, true);
            }

            return order.Status switch
            {
                ProxyOrderStatus.Draft => ("Đang soạn đơn", true, false, false, true),
                ProxyOrderStatus.Proposed => ("Chờ buyer duyệt", false, false, false, true),
                ProxyOrderStatus.Paid => ("Đã thanh toán - Sẵn sàng mua hàng", false, true, false, true),
                ProxyOrderStatus.InProgress => ("Đang mua hàng", false, false, true, false),
                ProxyOrderStatus.Completed => ("Đã hoàn thành", false, false, false, false),
                ProxyOrderStatus.Cancelled => ("Đã hủy", false, false, false, false),
                ProxyOrderStatus.Expired => ("Đã hết hạn", false, false, false, false),
                _ => ("Không xác định", false, false, false, false)
            };
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

            req.ProxyShoppingOrderId = order.Id;
            req.UpdatedAt = DateTime.UtcNow;
            await _requestRepo.UpdateAsync(requestId, req);
            return order.Id;
        }

        // Advanced product search with weights - chỉ tìm trong chợ mà proxy đã đăng ký
        public async Task<List<object>> AdvancedProductSearchAsync(string proxyShopperId, string query, double wPrice, double wReputation, double wSold, double wStock)
        {
            try
            {
                Console.WriteLine($"[DEBUG] AdvancedProductSearchAsync - Starting for ProxyShopperId: {proxyShopperId}, Query: {query}");
                
                // Lấy thông tin proxy shopper registration để biết MarketId
                var proxyRegistration = await _proxyRepo.FindOneAsync(p => p.UserId == proxyShopperId && p.Status == "Approved");
                if (proxyRegistration == null)
                {
                    Console.WriteLine($"[DEBUG] AdvancedProductSearchAsync - No approved registration found for proxy: {proxyShopperId}");
                    return new List<object>();
                }

                Console.WriteLine($"[DEBUG] AdvancedProductSearchAsync - Proxy registered for MarketId: {proxyRegistration.MarketId}");

                // Lấy tất cả stores trong market mà proxy đã đăng ký
                var storesInMarket = await _storeRepo.FindManyAsync(s => s.MarketId == proxyRegistration.MarketId);
                var storeIdsInMarket = storesInMarket.Select(s => s.Id!).ToList();
                
                Console.WriteLine($"[DEBUG] AdvancedProductSearchAsync - Found {storeIdsInMarket.Count} stores in market");

                // Tìm sản phẩm theo tên trong các cửa hàng thuộc chợ mà proxy đã đăng ký
                var products = (await _productRepo.FindManyAsync(p => 
                    p.Name.ToLower().Contains(query.ToLower()) && 
                    storeIdsInMarket.Contains(p.StoreId)))?.ToList() ?? new List<Product>();
                
                Console.WriteLine($"[DEBUG] AdvancedProductSearchAsync - Found {products.Count} products in store");
                
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
                // double minRep = 0; // Tạm thời đặt = 0 vì chưa có trường reputation
                // double maxRep = 5; // Giả định thang điểm 0-5
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
                
                Console.WriteLine($"[DEBUG] AdvancedProductSearchAsync - Returning {result.Count} products");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AdvancedProductSearchAsync - Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] AdvancedProductSearchAsync - StackTrace: {ex.StackTrace}");
                return new List<object>();
            }
        }

        // 4. Proxy lên đơn, gửi đề xuất (điền sản phẩm thật + phí)
        public async Task<bool> SendProposalAsync(string orderId, ProxyShoppingProposalDTO proposal)
        {
            try
            {
                Console.WriteLine($"[DEBUG] SendProposalAsync - Starting with OrderId: {orderId}");
                
                var order = await _orderRepo.FindOneAsync(o => o.Id == orderId);
                if (order == null)
                {
                    Console.WriteLine($"[DEBUG] SendProposalAsync - Order not found: {orderId}");
                    return false;
                }

                Console.WriteLine($"[DEBUG] SendProposalAsync - Order found. Status: {order.Status}");
                Console.WriteLine($"[DEBUG] SendProposalAsync - Order BuyerId: {order.BuyerId}");
                Console.WriteLine($"[DEBUG] SendProposalAsync - Order ProxyShopperId: {order.ProxyShopperId}");

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

                await _orderRepo.UpdateAsync(orderId, order);
                Console.WriteLine($"[DEBUG] SendProposalAsync - Order updated successfully");

                // Tạo notification cho buyer
                try
                {
                    var buyerName = "Khách hàng";
                    var buyer = await _userRepo.FindOneAsync(u => u.Id == order.BuyerId);
                    if (buyer != null)
                    {
                        buyerName = buyer.FullName ?? "Khách hàng";
                    }

                    var title = "📋 Đề xuất đơn hàng mới!";
                    var message = $"Proxy shopper đã gửi đề xuất đơn hàng cho yêu cầu của bạn. " +
                                $"Tổng tiền: {order.TotalAmount:N0} VND, Phí proxy: {order.ProxyFee:N0} VND. " +
                                $"Hãy kiểm tra và duyệt đề xuất để tiến hành thanh toán.";
                    
                    await _notificationService.CreateNotificationAsync(
                        order.BuyerId,
                        title,
                        message,
                        "PROXY_SHOPPING_PROPOSAL_RECEIVED"
                    );

                    Console.WriteLine($"[INFO] SendProposalAsync - Created notification for buyer {order.BuyerId}");
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"[ERROR] SendProposalAsync - Failed to create notification: {notifEx.Message}");
                    // Không throw exception vì notification không phải critical operation
                }
                
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
            try
            {
                Console.WriteLine($"[DEBUG] BuyerApproveAndPayAsync - Starting with OrderId: {orderId}, BuyerId: {buyerId}");
                
                var order = await _orderRepo.FindOneAsync(o => o.Id == orderId && o.BuyerId == buyerId);
                if (order == null || order.Status != ProxyOrderStatus.Proposed)
                {
                    Console.WriteLine($"[DEBUG] BuyerApproveAndPayAsync - Order not found or invalid status. Order: {order?.Id}, Status: {order?.Status}");
                    return false;
                }

                Console.WriteLine($"[DEBUG] BuyerApproveAndPayAsync - Order found. ProxyShopperId: {order.ProxyShopperId}");

                // Thực hiện thanh toán ở đây (TODO)
                order.Status = ProxyOrderStatus.Paid;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepo.UpdateAsync(orderId, order);
                Console.WriteLine($"[DEBUG] BuyerApproveAndPayAsync - Order status updated to Paid");

                // Tạo notification cho proxy shopper
                try
                {
                    if (!string.IsNullOrEmpty(order.ProxyShopperId))
                    {
                        var proxyShopperName = "Proxy Shopper";
                        var proxyShopper = await _userRepo.FindOneAsync(u => u.Id == order.ProxyShopperId);
                        if (proxyShopper != null)
                        {
                            proxyShopperName = proxyShopper.FullName ?? "Proxy Shopper";
                        }

                        var title = "💰 Đơn hàng đã được duyệt và thanh toán!";
                        var message = $"Khách hàng đã duyệt đề xuất và hoàn tất thanh toán cho đơn hàng #{orderId.Substring(orderId.Length - 8)}. " +
                                    $"Tổng tiền: {order.TotalAmount:N0} VND, Phí của bạn: {order.ProxyFee:N0} VND. " +
                                    $"Bạn có thể bắt đầu mua hàng ngay bây giờ!";
                        
                        await _notificationService.CreateNotificationAsync(
                            order.ProxyShopperId,
                            title,
                            message,
                            "PROXY_SHOPPING_ORDER_APPROVED"
                        );

                        Console.WriteLine($"[INFO] BuyerApproveAndPayAsync - Created notification for proxy shopper {order.ProxyShopperId}");
                    }
                    else
                    {
                        Console.WriteLine($"[WARNING] BuyerApproveAndPayAsync - ProxyShopperId is null or empty for order {orderId}");
                    }
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"[ERROR] BuyerApproveAndPayAsync - Failed to create notification: {notifEx.Message}");
                    // Không throw exception vì notification không phải critical operation
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] BuyerApproveAndPayAsync - Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] BuyerApproveAndPayAsync - StackTrace: {ex.StackTrace}");
                return false;
            }
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
        public async Task<bool> UploadBoughtItemsAsync(string orderId, string proofImages, string? note)
        {
            try
            {
                Console.WriteLine($"[DEBUG] UploadBoughtItemsAsync - Starting with OrderId: {orderId}");
                
                var order = await _orderRepo.FindOneAsync(o => o.Id == orderId);
                if (order == null)
                {
                    Console.WriteLine($"[DEBUG] UploadBoughtItemsAsync - Order not found: {orderId}");
                    return false;
                }
                
                if (order.Status != ProxyOrderStatus.InProgress)
                {
                    Console.WriteLine($"[DEBUG] UploadBoughtItemsAsync - Order status is not InProgress: {order.Status}");
                    return false;
                }
                order.ProofImages = proofImages;
                order.Notes = note;
                order.UpdatedAt = DateTime.UtcNow;
                
                Console.WriteLine($"[DEBUG] UploadBoughtItemsAsync - Saving proof image to database: {order.ProofImages ?? "null"}");
                Console.WriteLine($"[DEBUG] UploadBoughtItemsAsync - Note: {note}");
                
                await _orderRepo.UpdateAsync(orderId, order);
                Console.WriteLine($"[DEBUG] UploadBoughtItemsAsync - Successfully updated order {orderId}");

                // Tạo notification cho buyer
                try
                {
                    var buyerName = "Khách hàng";
                    var buyer = await _userRepo.FindOneAsync(u => u.Id == order.BuyerId);
                    if (buyer != null)
                    {
                        buyerName = buyer.FullName ?? "Khách hàng";
                    }

                    var title = "🛍️ Proxy đã mua hàng thành công!";
                    var message = $"Proxy shopper đã hoàn tất việc mua sản phẩm của bạn và đã upload ảnh chứng từ. " +
                                $"Đơn hàng #{orderId.Substring(orderId.Length - 8)} sẵn sàng để giao. " +
                                $"Hãy kiểm tra ảnh chứng từ và xác nhận đã nhận hàng.";
                    
                    await _notificationService.CreateNotificationAsync(
                        order.BuyerId,
                        title,
                        message,
                        "PROXY_SHOPPING_PROOF_UPLOADED"
                    );

                    Console.WriteLine($"[INFO] UploadBoughtItemsAsync - Created notification for buyer {order.BuyerId}");
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"[ERROR] UploadBoughtItemsAsync - Failed to create notification: {notifEx.Message}");
                    // Không throw exception vì notification không phải critical operation
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UploadBoughtItemsAsync - Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] UploadBoughtItemsAsync - StackTrace: {ex.StackTrace}");
                return false;
            }
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

            // Lấy thông tin market
            Market? market = null;
            if (!string.IsNullOrEmpty(request.MarketId))
            {
                market = await _marketRepo.FindOneAsync(m => m.Id == request.MarketId);
            }
            
            return new ProxyRequestResponseDto
            {
                Id = request.Id,
                ProxyOrderId = request.ProxyShoppingOrderId,
                Items = request.Items,
                Status = request.Status.ToString(),
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                BuyerName = buyer?.FullName,
                BuyerEmail = buyer?.Email,
                BuyerPhone = buyer?.PhoneNumber,
                MarketId = request.MarketId,
                MarketName = market?.Name
            };
        }
    }
}
