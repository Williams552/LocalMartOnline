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
        private readonly IMapper _mapper;

        public ProxyShopperService(
            IRepository<ProxyShoppingOrder> orderRepo,
            IRepository<ProxyShopperRegistration> proxyRepo,
            IRepository<User> userRepo,
            IRepository<Product> productRepo,
            IRepository<Store> storeRepo,
            IRepository<ProxyRequest> requestRepo,
            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _proxyRepo = proxyRepo;
            _userRepo = userRepo;
            _productRepo = productRepo;
            _storeRepo = storeRepo;
            _requestRepo = requestRepo;
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

            req.ProxyShoppingOrderId = order.Id;
            req.UpdatedAt = DateTime.UtcNow;
            await _requestRepo.UpdateAsync(requestId, req);
            return order.Id;
        }

        // 4. Proxy lên đơn, gửi đề xuất (điền sản phẩm thật + phí)
        public async Task<bool> SendProposalAsync(string orderId, ProxyShoppingProposalDTO proposal)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId);
            if (order == null || order.Status != ProxyOrderStatus.Draft) return false;

            order.Items = proposal.Items;
            order.TotalAmount = proposal.TotalAmount;
            order.ProxyFee = proposal.ProxyFee;
            order.Notes = proposal.Note;
            order.Status = ProxyOrderStatus.Proposed;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepo.UpdateAsync(orderId, order);
            return true;
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
    }
}
