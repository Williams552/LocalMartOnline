using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.SupportRequest;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Services;
using MongoDB.Driver;

namespace LocalMartOnline.Services.Implement
{
    public class SupportRequestService : ISupportRequestService
    {
        private readonly MongoDBService _mongoDBService;
        private readonly IMongoCollection<SupportRequest> _supportRequests;
        private readonly IMongoCollection<User> _users;

        public SupportRequestService(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
            _supportRequests = _mongoDBService.GetCollection<SupportRequest>("SupportRequests");
            _users = _mongoDBService.GetCollection<User>("Users");
        }

        public async Task<IEnumerable<SupportRequestDto>> GetAllSupportRequestsAsync()
        {
            var supportRequests = await _supportRequests.Find(_ => true).ToListAsync();
            var supportRequestDtos = new List<SupportRequestDto>();

            foreach (var request in supportRequests)
            {
                var user = await _users.Find(u => u.Id == request.UserId).FirstOrDefaultAsync();
                supportRequestDtos.Add(MapToDto(request, user));
            }

            return supportRequestDtos.OrderByDescending(x => x.CreatedAt);
        }

        public async Task<IEnumerable<SupportRequestDto>> GetSupportRequestsByStatusAsync(string status)
        {
            var filter = Builders<SupportRequest>.Filter.Eq(sr => sr.Status, status);
            var supportRequests = await _supportRequests.Find(filter).ToListAsync();
            var supportRequestDtos = new List<SupportRequestDto>();

            foreach (var request in supportRequests)
            {
                var user = await _users.Find(u => u.Id == request.UserId).FirstOrDefaultAsync();
                supportRequestDtos.Add(MapToDto(request, user));
            }

            return supportRequestDtos.OrderByDescending(x => x.CreatedAt);
        }

        public async Task<SupportRequestDto?> GetSupportRequestByIdAsync(string id)
        {
            var supportRequest = await _supportRequests.Find(sr => sr.Id == id).FirstOrDefaultAsync();
            if (supportRequest == null) return null;

            var user = await _users.Find(u => u.Id == supportRequest.UserId).FirstOrDefaultAsync();
            return MapToDto(supportRequest, user);
        }

        public async Task<IEnumerable<SupportRequestDto>> GetSupportRequestsByUserIdAsync(string userId)
        {
            var filter = Builders<SupportRequest>.Filter.Eq(sr => sr.UserId, userId);
            var supportRequests = await _supportRequests.Find(filter).ToListAsync();
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

            return supportRequests
                .Select(request => MapToDto(request, user))
                .OrderByDescending(x => x.CreatedAt);
        }

        public async Task<string> CreateSupportRequestAsync(string userId, CreateSupportRequestDto createDto)
        {
            var supportRequest = new SupportRequest
            {
                UserId = userId,
                Subject = createDto.Subject,
                Description = createDto.Description,
                Status = SupportRequestStatus.Open.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _supportRequests.InsertOneAsync(supportRequest);
            return supportRequest.Id!;
        }

        public async Task<bool> RespondToSupportRequestAsync(string id, RespondToSupportRequestDto responseDto)
        {
            var filter = Builders<SupportRequest>.Filter.Eq(sr => sr.Id, id);
            var update = Builders<SupportRequest>.Update
                .Set(sr => sr.Response, responseDto.Response)
                .Set(sr => sr.Status, responseDto.Status)
                .Set(sr => sr.UpdatedAt, DateTime.UtcNow);

            var result = await _supportRequests.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateSupportRequestStatusAsync(string id, string status)
        {
            var filter = Builders<SupportRequest>.Filter.Eq(sr => sr.Id, id);
            var update = Builders<SupportRequest>.Update
                .Set(sr => sr.Status, status)
                .Set(sr => sr.UpdatedAt, DateTime.UtcNow);

            var result = await _supportRequests.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteSupportRequestAsync(string id)
        {
            var filter = Builders<SupportRequest>.Filter.Eq(sr => sr.Id, id);
            var result = await _supportRequests.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        private static SupportRequestDto MapToDto(SupportRequest supportRequest, User? user)
        {
            return new SupportRequestDto
            {
                Id = supportRequest.Id,
                UserId = supportRequest.UserId,
                UserName = user?.FullName ?? "Unknown User",
                UserEmail = user?.Email ?? "Unknown Email",
                Subject = supportRequest.Subject,
                Description = supportRequest.Description,
                Status = supportRequest.Status,
                Response = supportRequest.Response,
                CreatedAt = supportRequest.CreatedAt,
                UpdatedAt = supportRequest.UpdatedAt
            };
        }
    }
}
