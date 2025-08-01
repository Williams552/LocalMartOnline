using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.MarketFee;
using LocalMartOnline.Services.Interface;
using MongoDB.Driver;

namespace LocalMartOnline.Services.Implement
{
    public class MarketFeeTypeService : IMarketFeeTypeService
    {
        private readonly IMongoCollection<MarketFeeType> _marketFeeTypeCollection;

        public MarketFeeTypeService(IMongoDatabase database)
        {
            _marketFeeTypeCollection = database.GetCollection<MarketFeeType>("MarketFeeTypes");
        }

        public async Task<GetMarketFeeTypesResponseDto> GetAllMarketFeeTypesAsync()
        {
            var filter = Builders<MarketFeeType>.Filter.Eq(x => x.IsDeleted, false);
            var marketFeeTypes = await _marketFeeTypeCollection
                .Find(filter)
                .SortBy(x => x.FeeType)
                .ToListAsync();

            var dtos = marketFeeTypes.Select(MapToDto).ToList();

            return new GetMarketFeeTypesResponseDto
            {
                MarketFeeTypes = dtos,
                TotalCount = dtos.Count
            };
        }

        public async Task<MarketFeeTypeDto?> GetMarketFeeTypeByIdAsync(string id)
        {
            var filter = Builders<MarketFeeType>.Filter.And(
                Builders<MarketFeeType>.Filter.Eq(x => x.Id, id),
                Builders<MarketFeeType>.Filter.Eq(x => x.IsDeleted, false)
            );

            var marketFeeType = await _marketFeeTypeCollection
                .Find(filter)
                .FirstOrDefaultAsync();

            return marketFeeType != null ? MapToDto(marketFeeType) : null;
        }

        public async Task<MarketFeeTypeDto> CreateMarketFeeTypeAsync(CreateMarketFeeTypeDto createDto)
        {
            var marketFeeType = new MarketFeeType
            {
                FeeType = createDto.FeeType,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _marketFeeTypeCollection.InsertOneAsync(marketFeeType);

            return MapToDto(marketFeeType);
        }

        public async Task<MarketFeeTypeDto?> UpdateMarketFeeTypeAsync(string id, UpdateMarketFeeTypeDto updateDto)
        {
            var filter = Builders<MarketFeeType>.Filter.And(
                Builders<MarketFeeType>.Filter.Eq(x => x.Id, id),
                Builders<MarketFeeType>.Filter.Eq(x => x.IsDeleted, false)
            );

            var update = Builders<MarketFeeType>.Update
                .Set(x => x.FeeType, updateDto.FeeType);

            var result = await _marketFeeTypeCollection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<MarketFeeType>
                {
                    ReturnDocument = ReturnDocument.After
                });

            return result != null ? MapToDto(result) : null;
        }

        public async Task<bool> DeleteMarketFeeTypeAsync(string id)
        {
            var filter = Builders<MarketFeeType>.Filter.Eq(x => x.Id, id);
            var update = Builders<MarketFeeType>.Update
                .Set(x => x.IsDeleted, true);

            var result = await _marketFeeTypeCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RestoreMarketFeeTypeAsync(string id)
        {
            var filter = Builders<MarketFeeType>.Filter.Eq(x => x.Id, id);
            var update = Builders<MarketFeeType>.Update
                .Set(x => x.IsDeleted, false);

            var result = await _marketFeeTypeCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        private static MarketFeeTypeDto MapToDto(MarketFeeType marketFeeType)
        {
            return new MarketFeeTypeDto
            {
                Id = marketFeeType.Id,
                FeeType = marketFeeType.FeeType,
                CreatedAt = marketFeeType.CreatedAt,
                IsDeleted = marketFeeType.IsDeleted
            };
        }
    }
}
