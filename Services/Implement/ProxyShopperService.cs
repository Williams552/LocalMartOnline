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

namespace LocalMartOnline.Services.Implement
{
    public class ProxyShopperService : IProxyShopperService
    {
        private readonly IRepository<ProxyShoppingOrder> _orderRepo;
        private readonly IRepository<ProxyShopperRegistration> _proxyRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Store> _storeRepo;
        private readonly IMapper _mapper;

        public ProxyShopperService(
            IRepository<ProxyShoppingOrder> orderRepo,
            IRepository<ProxyShopperRegistration> proxyRepo,
            IRepository<User> userRepo,
            IRepository<Product> productRepo,
            IRepository<Store> storeRepo,
            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _proxyRepo = proxyRepo;
            _userRepo = userRepo;
            _productRepo = productRepo;
            _storeRepo = storeRepo;
            _mapper = mapper;
        }

        public async Task RegisterProxyShopperAsync(ProxyShopperRegistrationRequestDTO dto, string userId)
        {
            var registration = _mapper.Map<ProxyShopperRegistration>(dto);
            registration.UserId = userId!;
            registration.Status = "Pending";
            registration.CreatedAt = DateTime.UtcNow;
            registration.UpdatedAt = DateTime.UtcNow;
            await _proxyRepo.CreateAsync(registration);
        }

        public async Task<ProxyShopperRegistration?> GetMyRegistrationAsync(string userId)
        {
            return await _proxyRepo.FindOneAsync(r => r.UserId == userId);
        }

        public async Task<List<ProxyShopperRegistration>> GetAllRegistrationsAsync()
        {
            var regs = await _proxyRepo.GetAllAsync();
            return regs.ToList();
        }

        public async Task<bool> ApproveRegistrationAsync(ProxyShopperRegistrationApproveDTO dto)
        {
            var reg = await _proxyRepo.FindOneAsync(r => r.Id == dto.RegistrationId);
            if (reg == null) return false;
            reg.Status = dto.Approve ? "Approved" : "Rejected";
            reg.RejectionReason = dto.Approve ? null : dto.RejectionReason;
            reg.UpdatedAt = DateTime.UtcNow;
            await _proxyRepo.UpdateAsync(reg.Id!, reg);
            return true;
        }

        public async Task<List<ProxyShoppingOrder>> GetAvailableOrdersAsync()
        {
            // Lấy danh sách đơn hàng chưa có proxy shopper nhận
            var orders = await _orderRepo.FindManyAsync(o => o.Status == "Pending" && o.ProxyShopperId == null);
            return orders.ToList();
        }

        public async Task<bool> AcceptOrderAsync(string orderId, string proxyShopperId)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId);
            if (order == null || order.Status != "Pending") return false;
            order.ProxyShopperId = proxyShopperId;
            order.Status = "Accepted";
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(orderId, order);
            return true;
        }

        public async Task<bool> SendProposalAsync(string orderId, ProxyShoppingProposalDTO proposal)
        {
            // Gửi đề xuất cho người mua: lưu thông tin đề xuất vào đơn hàng
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId);
            if (order == null || order.Status != "Accepted") return false;
            order.TotalAmount = proposal.TotalAmount;
            order.ProxyFee = proposal.ProxyFee;
            order.Notes = proposal.Note;
            order.UpdatedAt = DateTime.UtcNow;
            // Nếu cần lưu chi tiết sản phẩm, có thể mở rộng model
            await _orderRepo.UpdateAsync(orderId, order);
            return true;
        }

        public async Task<bool> ConfirmOrderAsync(string orderId, string proxyShopperId)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId && o.ProxyShopperId == proxyShopperId);
            if (order == null || order.Status != "Accepted") return false;
            order.Status = "Confirmed";
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(orderId, order);
            return true;
        }

        public async Task<bool> UploadBoughtItemsAsync(string orderId, List<string> imageUrls, string note)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId);
            if (order == null || order.Status != "Confirmed") return false;
            // Nếu cần lưu ảnh, có thể mở rộng model
            order.Notes = note;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(orderId, order);
            return true;
        }

        public async Task<bool> ConfirmFinalPriceAsync(string orderId, decimal finalPrice)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId);
            if (order == null) return false;
            order.TotalAmount = finalPrice;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(orderId, order);
            return true;
        }

        public async Task<bool> ConfirmDeliveryAsync(string orderId)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId);
            if (order == null || order.Status != "Confirmed") return false;
            
            // Cập nhật trạng thái đơn hàng
            order.Status = "Completed";
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(orderId, order);

            // Tăng PurchaseCount cho các sản phẩm trong đơn hàng proxy shopping
            if (order.Items != null)
            {
                var productIds = order.Items.Select(item => item.Id).Distinct().ToList();
                foreach (var productId in productIds)
                {
                    if (string.IsNullOrEmpty(productId)) continue;
                    var product = await _productRepo.FindOneAsync(p => p.Id == productId);
                    if (product != null)
                    {
                        product.PurchaseCount += 1; // Tăng 1 lần mua (không phụ thuộc số lượng)
                        await _productRepo.UpdateAsync(product.Id!, product);
                    }
                }
            }

            return true;
        }

        public async Task<bool> ReplaceOrRemoveProductAsync(string orderId, string productName, ProductDto? replacementItem = null)
        {
            var order = await _orderRepo.FindOneAsync(o => o.Id == orderId);
            if (order == null || order.Items == null) return false;
            var idx = order.Items.FindIndex(i => i.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));
            if (idx == -1) return false;
            if (replacementItem == null)
            {
                // Xóa sản phẩm
                order.Items.RemoveAt(idx);
            }
            else
            {
                // Thay thế sản phẩm
                order.Items[idx] = replacementItem;
            }
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(orderId, order);
            return true;
        }

        public async Task<List<ProductDto>> SmartSearchProductsAsync(string query, int limit = 10)
        {
            // Tìm sản phẩm theo từ khóa
            var products = (await _productRepo.FindManyAsync(p => p.Name.Contains(query) && p.Status == ProductStatus.Active)).ToList();
            if (!products.Any()) return new List<ProductDto>();

            // Lấy thông tin store cho từng sản phẩm
            var storeIds = products.Select(p => p.StoreId).Distinct().ToList();
            var stores = (await _storeRepo.FindManyAsync(s => storeIds.Contains(s.Id!))).ToList();

            // Tìm min/max cho price và purchase_count
            var minPrice = products.Min(p => p.Price);
            var maxPrice = products.Max(p => p.Price);
            var minPurchase = products.Min(p => p.PurchaseCount);
            var maxPurchase = products.Max(p => p.PurchaseCount);

            // Chuẩn hóa và tính score
            var result = products.Select(p =>
            {
                var store = stores.FirstOrDefault(s => s.Id == p.StoreId);
                decimal priceNorm = (maxPrice == minPrice) ? 1 : 1 - (p.Price - minPrice) / (maxPrice - minPrice);
                decimal ratingNorm = store != null ? ((store.Rating - 1m) / 4m) : 0;
                decimal purchaseNorm = (maxPurchase == minPurchase) ? 1 : (p.PurchaseCount - minPurchase) / (decimal)(maxPurchase - minPurchase);
                decimal score = 0.5m * priceNorm + 0.3m * ratingNorm + 0.2m * purchaseNorm;
                return new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Unit = p.UnitId,
                    PurchaseCount = p.PurchaseCount,
                    Score = Math.Round(score, 2),
                    Seller = new SellerDto
                    {
                        Name = store?.Name ?? "",
                        Rating = store?.Rating ?? 0,
                        Market = store?.MarketId ?? ""
                    }
                };
            })
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .ToList();

            return result;
        }
    }
}
