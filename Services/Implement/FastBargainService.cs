using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.FastBargain;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Implement
{
    public class FastBargainService : IFastBargainService
    {
        private readonly IRepository<FastBargain> _repository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductImage> _productImageRepository;
        private readonly IRepository<ProductUnit> _productUnitRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Store> _storeRepository;
        

        public FastBargainService(IRepository<FastBargain> repository, IRepository<Product> productRepository, IRepository<ProductImage> productImageRepository, IRepository<ProductUnit> productUnitRepository, IRepository<User> userRepository, IRepository<Store> storeRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
            _productUnitRepository = productUnitRepository;
            _userRepository = userRepository;
            _storeRepository = storeRepository;
        }

        public async Task<FastBargainResponseDTO> StartBargainAsync(FastBargainCreateRequestDTO request)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
                throw new Exception("Product not found");
                
            // Get store to get the actual seller ID
            var store = await _storeRepository.GetByIdAsync(product.StoreId);
            if (store == null)
                throw new Exception("Store not found");
                
            var bargain = new FastBargain
            {
                ProductId = request.ProductId,
                BuyerId = request.BuyerId,
                SellerId = store.SellerId, // Get sellerId from store's SellerId
                Quantity = request.Quantity,
                Status = FastBargainStatus.Pending,
                CreatedAt = DateTime.Now,
                Proposals = new List<FastBargainProposal>
                {
                    new FastBargainProposal
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = request.BuyerId,
                        ProposedPrice = request.InitialOfferPrice,
                        ProposedAt = DateTime.Now
                    }
                },
                ProposalCount = 1,
            };
            await _repository.CreateAsync(bargain);
            return ToResponseDTO(bargain);
        }

        public async Task<FastBargainResponseDTO> ProposeAsync(FastBargainProposalDTO proposal)
        {
            var bargain = await _repository.GetByIdAsync(proposal.BargainId);
            if (bargain == null || bargain.Status != FastBargainStatus.Pending)
                throw new Exception("Bargain not found or not active");
            bargain.Proposals.Add(new FastBargainProposal
            {
                Id = Guid.NewGuid().ToString(),
                UserId = proposal.UserId,
                ProposedPrice = proposal.ProposedPrice,
                ProposedAt = DateTime.Now
            });
            bargain.ProposalCount++;
            await _repository.UpdateAsync(bargain.Id ?? string.Empty, bargain);
            return ToResponseDTO(bargain);
        }

        public async Task<FastBargainResponseDTO> TakeActionAsync(FastBargainActionRequestDTO request)
        {
            var bargain = await _repository.GetByIdAsync(request.BargainId);
            if (bargain == null || bargain.Status != FastBargainStatus.Pending)
                throw new Exception("Bargain not found or not active");
            if (request.Action == "Accept")
            {
                bargain.Status = FastBargainStatus.Accepted;
                bargain.ClosedAt = DateTime.Now;
                bargain.FinalPrice = bargain.Proposals.LastOrDefault()?.ProposedPrice;
            }
            else if (request.Action == "Reject")
            {
                bargain.Status = FastBargainStatus.Rejected;
                bargain.ClosedAt = DateTime.Now;
            }
            else if (request.Action == "Counter" && request.CounterPrice.HasValue)
            {
                bargain.Proposals.Add(new FastBargainProposal
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = request.UserId,
                    ProposedPrice = request.CounterPrice.Value,
                    ProposedAt = DateTime.Now
                });
                bargain.ProposalCount++;
            }
            await _repository.UpdateAsync(bargain.Id ?? string.Empty, bargain);
            return ToResponseDTO(bargain);
        }

        public async Task<FastBargainResponseDTO?> GetByIdAsync(string bargainId)
        {
            var bargain = await _repository.GetByIdAsync(bargainId);
            return bargain == null ? null : ToResponseDTO(bargain);
        }

        public async Task<List<FastBargainResponseDTO>> GetByUserIdAsync(string userId)
        {
            var bargains = await _repository.FindManyAsync(b => b.BuyerId == userId);
            return bargains.Select(ToResponseDTO).ToList();
        }

        public async Task<List<FastBargainResponseDTO>> GetMyBargainsAsync(string userId, string userRole)
        {
            IEnumerable<FastBargain> bargains;
            
            // Phân quyền theo role
            if (userRole.Equals("Buyer", StringComparison.OrdinalIgnoreCase))
            {
                // Buyer chỉ thấy bargains mà họ là người mua
                bargains = await _repository.FindManyAsync(b => b.BuyerId == userId);
            }
            else if (userRole.Equals("Seller", StringComparison.OrdinalIgnoreCase))
            {
                // Seller chỉ thấy bargains mà họ là người bán
                bargains = await _repository.FindManyAsync(b => b.SellerId == userId);
            }
            else
            {
                // Các role khác (Admin, etc.) có thể thấy tất cả bargains của user
                bargains = await _repository.FindManyAsync(b => b.BuyerId == userId || b.SellerId == userId);
            }
            
            return bargains.Select(b => ToResponseDTO(b, userId, userRole)).ToList();
        }

        public async Task<List<FastBargainResponseDTO>> GetAllPendingBargainsAsync()
        {
            var bargains = await _repository.GetAllAsync();
            return bargains.Select(ToResponseDTO).ToList();
        }

        public async Task<List<FastBargainResponseDTO>> GetPendingBargainsBySellerIdAsync(string sellerId)
        {
            var bargains = await _repository.FindManyAsync(b => b.SellerId == sellerId);
            return bargains.Select(ToResponseDTO).ToList();
        }

        private FastBargainResponseDTO ToResponseDTO(FastBargain bargain)
        {
            return ToResponseDTO(bargain, null, null);
        }

        private FastBargainResponseDTO ToResponseDTO(FastBargain bargain, string? currentUserId, string? currentUserRole)
        {
            var product = _productRepository.GetByIdAsync(bargain.ProductId).GetAwaiter().GetResult();
            var productImages = _productImageRepository.FindManyAsync(img => img.ProductId == bargain.ProductId).GetAwaiter().GetResult();
            var imageUrls = productImages.Select(img => img.ImageUrl).ToList();
            
            // Xác định role của user hiện tại trong bargain này
            string userRole = string.Empty;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                if (bargain.BuyerId == currentUserId)
                    userRole = "Buyer";
                else if (bargain.SellerId == currentUserId)
                    userRole = "Seller";
                else
                    userRole = currentUserRole ?? string.Empty;
            }
            
            // Get product unit name
            var productUnit = product?.UnitId != null ? 
                _productUnitRepository.GetByIdAsync(product.UnitId).GetAwaiter().GetResult() : 
                null;
            
            // Get buyer information
            var buyer = _userRepository.GetByIdAsync(bargain.BuyerId).GetAwaiter().GetResult();
            
            // Get seller information
            var seller = _userRepository.GetByIdAsync(bargain.SellerId).GetAwaiter().GetResult();
            
            // Get store information
            var store = _storeRepository.GetByIdAsync(product?.StoreId ?? string.Empty).GetAwaiter().GetResult();
            
            return new FastBargainResponseDTO
            {
                BargainId = bargain.Id ?? string.Empty,
                Status = bargain.Status.ToString(),
                FinalPrice = bargain.FinalPrice,
                ProductName = product?.Name ?? string.Empty,
                OriginalPrice = product?.Price,
                Quantity = bargain.Quantity,
                ProductUnitName = productUnit?.Name ?? string.Empty,
                BuyerName = buyer?.FullName ?? string.Empty,
                SellerName = seller?.FullName ?? string.Empty,
                StoreName = store?.Name ?? string.Empty,
                ProductImages = imageUrls,
                BuyerId = bargain.BuyerId,
                SellerId = bargain.SellerId,
                UserRole = userRole,
                Proposals = bargain.Proposals.Select(p => new FastBargainProposalDTO
                {
                    BargainId = bargain.Id ?? string.Empty,
                    UserId = p.UserId,
                    ProposedPrice = p.ProposedPrice,
                    ProposedAt = p.ProposedAt
                }).ToList()
            };
        }
    }
}
