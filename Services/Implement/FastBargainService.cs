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

        public FastBargainService(IRepository<FastBargain> repository, IRepository<Product> productRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
        }

        public async Task<FastBargainResponseDTO> StartBargainAsync(FastBargainCreateRequestDTO request)
        {
            // TODO: Validate product, check inventory, check for existing active bargain, etc.
            var bargain = new FastBargain
            {
                // Id = null, để MongoDB tự sinh ObjectId
                ProductId = request.ProductId,
                BuyerId = request.BuyerId,
                // SellerId: get from product
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
                // ExpiresAt: set timeout
            };
            await _repository.CreateAsync(bargain);
            return ToResponseDTO(bargain);
        }

        public async Task<FastBargainResponseDTO> ProposeAsync(FastBargainProposalDTO proposal)
        {
            var bargain = await _repository.GetByIdAsync(proposal.BargainId);
            if (bargain == null || bargain.Status != FastBargainStatus.Pending)
                throw new Exception("Bargain not found or not active");
            // TODO: Check proposal limit, race condition, etc.
            bargain.Proposals.Add(new FastBargainProposal
            {
                Id = Guid.NewGuid().ToString(),
                UserId = proposal.UserId,
                ProposedPrice = proposal.ProposedPrice,
                ProposedAt = DateTime.Now
            });
            bargain.ProposalCount++;
            await _repository.UpdateAsync(bargain.Id, bargain);
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
            // TODO: Notification, timeout, abuse check, etc.
            await _repository.UpdateAsync(bargain.Id, bargain);
            return ToResponseDTO(bargain);
        }

        public async Task<FastBargainResponseDTO?> GetByIdAsync(string bargainId)
        {
            var bargain = await _repository.GetByIdAsync(bargainId);
            return bargain == null ? null : ToResponseDTO(bargain);
        }

        public async Task<List<FastBargainResponseDTO>> GetByUserIdAsync(string userId)
        {
            var bargains = await _repository.FindManyAsync(b => b.BuyerId == userId || b.SellerId == userId);
            return bargains.Select(ToResponseDTO).ToList();
        }

        private FastBargainResponseDTO ToResponseDTO(FastBargain bargain)
        {
            // Synchronously get product info (for async, refactor all usages to async)
            var product = _productRepository.GetByIdAsync(bargain.ProductId).GetAwaiter().GetResult();
            return new FastBargainResponseDTO
            {
                BargainId = bargain.Id,
                Status = bargain.Status.ToString(),
                FinalPrice = bargain.FinalPrice,
                ProductName = product?.Name ?? string.Empty,
                OriginalPrice = product?.Price,
                Proposals = bargain.Proposals.Select(p => new FastBargainProposalDTO
                {
                    BargainId = bargain.Id,
                    UserId = p.UserId,
                    ProposedPrice = p.ProposedPrice,
                    ProposedAt = p.ProposedAt
                }).ToList()
            };
        }
    }
}
