using AutoMapper;
using MongoDB.Driver;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace LocalMartOnline.Services.Implement
{
    public class MarketFeeService : IMarketFeeService
    {
        private readonly IRepository<MarketFee> _repo;
        private readonly IRepository<Market> _marketRepo;
        private readonly IMongoCollection<Market> _marketCollection;
        private readonly IMongoCollection<MarketFee> _marketFeeCollection;
        private readonly IMongoCollection<MarketFeeType> _marketFeeTypeCollection;
        private readonly IMapper _mapper;
        public MarketFeeService(IRepository<MarketFee> repo, IRepository<Market> marketRepo, IMapper mapper, IMongoDatabase database)
        {
            _repo = repo;
            _marketRepo = marketRepo;
            _mapper = mapper;
            _marketCollection = database.GetCollection<Market>("Markets");
            _marketFeeCollection = database.GetCollection<MarketFee>("MarketFees");
            _marketFeeTypeCollection = database.GetCollection<MarketFeeType>("MarketFeeTypes");
        }

        public async Task<IEnumerable<MarketFeeDto>> GetAllAsync(GetMarketFeeRequestDto request)
        {
            var filterBuilder = Builders<MarketFee>.Filter;
            var filter = filterBuilder.Empty;

            // Filter by MarketId if provided
            if (!string.IsNullOrEmpty(request.MarketFeeId))
            {
                filter = filterBuilder.And(filter, 
                    filterBuilder.Eq(mr => mr.MarketId, request.MarketFeeId));
            }

            // Filter by search keyword if provided
            if (!string.IsNullOrEmpty(request.SearchKeyword))
            {
                filter = filterBuilder.And(filter, 
                    filterBuilder.Or(
                        filterBuilder.Regex(mr => mr.Name, 
                            new MongoDB.Bson.BsonRegularExpression(request.SearchKeyword, "i")),
                        filterBuilder.Regex(mr => mr.Description, 
                            new MongoDB.Bson.BsonRegularExpression(request.SearchKeyword, "i"))
                    ));
            }

            var totalCount = await _marketFeeCollection.CountDocumentsAsync(filter);

            var marketFees = await _marketFeeCollection
                .Find(filter)
                .Sort(Builders<MarketFee>.Sort.Descending(mf => mf.CreatedAt))
                .ToListAsync();

            // Get market information for each fee
            var marketIds = marketFees.Select(mf => mf.MarketId).Distinct().ToList();
            var markets = await _marketCollection
                .Find(Builders<Market>.Filter.In(m => m.Id, marketIds))
                .ToListAsync();
            
            var marketDict = markets.ToDictionary(m => m.Id!, m => m.Name);

            // Get market fee type information for each fee
            var marketFeeTypeIds = marketFees.Select(mf => mf.MarketFeeTypeId).Distinct().ToList();
            var marketFeeTypes = await _marketFeeTypeCollection
                .Find(Builders<MarketFeeType>.Filter.In(mft => mft.Id, marketFeeTypeIds))
                .ToListAsync();
            
            var marketFeeTypeDict = marketFeeTypes.ToDictionary(mft => mft.Id!, mft => mft.FeeType);

            var marketFeeDtos = marketFees.Select(mf => new MarketFeeDto
            {
                Id = mf.Id!,
                MarketId = mf.MarketId,
                MarketName = marketDict.GetValueOrDefault(mf.MarketId, "Unknown Market"),
                MarketFeeTypeId = mf.MarketFeeTypeId,
                MarketFeeTypeName = marketFeeTypeDict.GetValueOrDefault(mf.MarketFeeTypeId, "Unknown Type"),
                Name = mf.Name,
                Amount = mf.Amount,
                Description = mf.Description,
                PaymentDay = mf.PaymentDay,
                CreatedAt = mf.CreatedAt
            }).ToList();

            return marketFeeDtos;
        }

        public async Task<MarketFeeDto?> GetByIdAsync(string id)
        {
            var fee = await _repo.GetByIdAsync(id);
            if (fee == null) return null;
            
            var feeDto = _mapper.Map<MarketFeeDto>(fee);
            
            // Get market name
            var market = await _marketRepo.GetByIdAsync(fee.MarketId);
            if (market != null)
            {
                feeDto.MarketName = market.Name;
            }
            
            // Get market fee type name
            var marketFeeType = await _marketFeeTypeCollection
                .Find(Builders<MarketFeeType>.Filter.Eq(mft => mft.Id, fee.MarketFeeTypeId))
                .FirstOrDefaultAsync();
            if (marketFeeType != null)
            {
                feeDto.MarketFeeTypeName = marketFeeType.FeeType;
            }
            
            return feeDto;
        }

        public async Task<MarketFeeDto> CreateAsync(MarketFeeCreateDto dto)
        {
            var fee = _mapper.Map<MarketFee>(dto);
            fee.CreatedAt = DateTime.Now;
            fee.UpdatedAt = DateTime.Now;
            await _repo.CreateAsync(fee);
            
            var feeDto = _mapper.Map<MarketFeeDto>(fee);
            
            // Get market name
            var market = await _marketRepo.GetByIdAsync(fee.MarketId);
            if (market != null)
            {
                feeDto.MarketName = market.Name;
            }
            
            return feeDto;
        }

        public async Task<MarketFeeDto> CreateAsync(MarketFeeDto dto)
        {
            // Map từ MarketFeeDto sang MarketFee entity
            var fee = _mapper.Map<MarketFee>(dto);
            fee.CreatedAt = DateTime.Now;
            fee.UpdatedAt = DateTime.Now;
            await _repo.CreateAsync(fee);
            
            var feeDto = _mapper.Map<MarketFeeDto>(fee);
            
            // Get market name
            var market = await _marketRepo.GetByIdAsync(fee.MarketId);
            if (market != null)
            {
                feeDto.MarketName = market.Name;
            }
            
            return feeDto;
        }

        public async Task<bool> UpdateAsync(string id, MarketFeeUpdateDto dto)
        {
            var fee = await _repo.GetByIdAsync(id);
            if (fee == null) return false;
            
            if (dto.MarketFeeTypeId != null) fee.MarketFeeTypeId = dto.MarketFeeTypeId;
            if (dto.Name != null) fee.Name = dto.Name;
            if (dto.Amount != null) fee.Amount = dto.Amount.Value;
            if (dto.Description != null) fee.Description = dto.Description;
            if (dto.PaymentDay != null) fee.PaymentDay = dto.PaymentDay.Value;
            
            fee.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(id, fee);
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var fee = await _repo.GetByIdAsync(id);
            if (fee == null) return false;
            await _repo.DeleteAsync(id);
            return true;
        }

        public Task<bool> PayFeeAsync(MarketFeePaymentDto dto)
        {
            // TODO: Tích hợp thanh toán online thực tế (VnPay, v.v.)
            // Ở đây chỉ giả lập đã thanh toán thành công
            return Task.FromResult(true);
        }
    }
}
