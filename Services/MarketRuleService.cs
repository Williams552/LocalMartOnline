using MongoDB.Driver;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.MarketRule;

namespace LocalMartOnline.Services
{
    public class MarketRuleService : IMarketRuleService
    {
        private readonly IMongoCollection<MarketRule> _marketRuleCollection;
        private readonly IMongoCollection<Market> _marketCollection;
        private readonly IMongoCollection<User> _userCollection;

        public MarketRuleService(IMongoDatabase database)
        {
            _marketRuleCollection = database.GetCollection<MarketRule>("market_rules");
            _marketCollection = database.GetCollection<Market>("markets");
            _userCollection = database.GetCollection<User>("users");
        }

        public async Task<GetMarketRulesResponseDto> GetMarketRulesAsync(GetMarketRulesRequestDto request)
        {
            var filterBuilder = Builders<MarketRule>.Filter;
            var filter = filterBuilder.Empty;

            // Filter by market ID if provided
            if (!string.IsNullOrEmpty(request.MarketId))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(mr => mr.MarketId, request.MarketId));
            }

            // Filter by search keyword if provided
            if (!string.IsNullOrEmpty(request.SearchKeyword))
            {
                filter = filterBuilder.And(filter, 
                    filterBuilder.Regex(mr => mr.Description, 
                        new MongoDB.Bson.BsonRegularExpression(request.SearchKeyword, "i")));
            }

            var totalCount = await _marketRuleCollection.CountDocumentsAsync(filter);

            var marketRules = await _marketRuleCollection
                .Find(filter)
                .Sort(Builders<MarketRule>.Sort.Descending(mr => mr.CreatedAt))
                .Skip((request.Page - 1) * request.PageSize)
                .Limit(request.PageSize)
                .ToListAsync();

            // Get market information for each rule
            var marketIds = marketRules.Select(mr => mr.MarketId).Distinct().ToList();
            var markets = await _marketCollection
                .Find(Builders<Market>.Filter.In(m => m.Id, marketIds))
                .ToListAsync();
            
            var marketDict = markets.ToDictionary(m => m.Id!, m => m.Name);

            var marketRuleDtos = marketRules.Select(mr => new MarketRuleDto
            {
                Id = mr.Id!,
                MarketId = mr.MarketId,
                MarketName = marketDict.GetValueOrDefault(mr.MarketId, "Unknown Market"),
                Description = mr.Description,
                CreatedAt = mr.CreatedAt,
                UpdatedAt = mr.UpdatedAt
            }).ToList();

            return new GetMarketRulesResponseDto
            {
                MarketRules = marketRuleDtos,
                TotalCount = (int)totalCount,
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        public async Task<MarketRuleDto?> GetMarketRuleByIdAsync(string ruleId)
        {
            var marketRule = await _marketRuleCollection
                .Find(Builders<MarketRule>.Filter.Eq(mr => mr.Id, ruleId))
                .FirstOrDefaultAsync();

            if (marketRule == null)
                return null;

            var market = await _marketCollection
                .Find(Builders<Market>.Filter.Eq(m => m.Id, marketRule.MarketId))
                .FirstOrDefaultAsync();

            return new MarketRuleDto
            {
                Id = marketRule.Id!,
                MarketId = marketRule.MarketId,
                MarketName = market?.Name ?? "Unknown Market",
                Description = marketRule.Description,
                CreatedAt = marketRule.CreatedAt,
                UpdatedAt = marketRule.UpdatedAt
            };
        }

        public async Task<GetMarketRulesResponseDto> GetMarketRulesByMarketIdAsync(string marketId, int page = 1, int pageSize = 10)
        {
            var request = new GetMarketRulesRequestDto
            {
                MarketId = marketId,
                Page = page,
                PageSize = pageSize
            };

            return await GetMarketRulesAsync(request);
        }

        public async Task<MarketRuleDto?> CreateMarketRuleAsync(string userId, CreateMarketRuleDto createMarketRuleDto)
        {
            // Validate market exists
            if (!await IsValidMarketIdAsync(createMarketRuleDto.MarketId))
                return null;

            var marketRule = new MarketRule
            {
                MarketId = createMarketRuleDto.MarketId,
                Description = createMarketRuleDto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _marketRuleCollection.InsertOneAsync(marketRule);

            return await GetMarketRuleByIdAsync(marketRule.Id!);
        }

        public async Task<MarketRuleDto?> UpdateMarketRuleAsync(string userId, string ruleId, UpdateMarketRuleDto updateMarketRuleDto)
        {
            var filter = Builders<MarketRule>.Filter.Eq(mr => mr.Id, ruleId);
            var update = Builders<MarketRule>.Update
                .Set(mr => mr.Description, updateMarketRuleDto.Description)
                .Set(mr => mr.UpdatedAt, DateTime.UtcNow);

            var result = await _marketRuleCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
                return null;

            return await GetMarketRuleByIdAsync(ruleId);
        }

        public async Task<bool> DeleteMarketRuleAsync(string userId, string ruleId)
        {
            var filter = Builders<MarketRule>.Filter.Eq(mr => mr.Id, ruleId);
            var result = await _marketRuleCollection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> IsValidMarketIdAsync(string marketId)
        {
            var market = await _marketCollection
                .Find(Builders<Market>.Filter.Eq(m => m.Id, marketId))
                .FirstOrDefaultAsync();

            return market != null;
        }

        public async Task<bool> CanUserManageMarketRuleAsync(string userId, string userRole)
        {
            // All specified roles can manage market rules
            var allowedRoles = new[]
            {
                "Admin",
                "MarketManagementBoardHead",
                "LocalGovernmentRepresentative",
                "Buyer",
                "ProxyShopper",
                "MarketStaff",
                "Seller"
            };

            return allowedRoles.Contains(userRole);
        }
    }
}
