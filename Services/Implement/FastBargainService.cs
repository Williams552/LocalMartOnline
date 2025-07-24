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
        

        public FastBargainService(IRepository<FastBargain> repository, IRepository<Product> productRepository, IRepository<ProductImage> productImageRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
        }

        public async Task<FastBargainResponseDTO> StartBargainAsync(FastBargainCreateRequestDTO request)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
                throw new Exception("Product not found");
                
            var bargain = new FastBargain
            {
                ProductId = request.ProductId,
                BuyerId = request.BuyerId,
                SellerId = product.StoreId, // Get sellerId from product's StoreId
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
            var product = _productRepository.GetByIdAsync(bargain.ProductId).GetAwaiter().GetResult();
            var productImages = _productImageRepository.FindManyAsync(img => img.ProductId == bargain.ProductId).GetAwaiter().GetResult();
            var imageUrls = productImages.Select(img => img.ImageUrl).ToList();
            
            return new FastBargainResponseDTO
            {
                BargainId = bargain.Id ?? string.Empty,
                Status = bargain.Status.ToString(),
                FinalPrice = bargain.FinalPrice,
                ProductName = product?.Name ?? string.Empty,
                OriginalPrice = product?.Price,
                ProductImages = imageUrls,
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
