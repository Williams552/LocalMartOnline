using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Chat;
using MongoDB.Driver;
using LocalMartOnline.Services.Interface;

namespace LocalMartOnline.Services.Implement
{
    public class ChatService : IChatService
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Chat> _chatCollection;
        private readonly IMongoCollection<User> _userCollection;

        public ChatService(IMongoDatabase database)
        {
            _database = database;
            _chatCollection = _database.GetCollection<Chat>("Chats");
            _userCollection = _database.GetCollection<User>("Users");
        }

        public async Task<ChatMessageDto> SendMessageAsync(string senderId, SendMessageRequestDto request)
        {
            var chat = new Chat
            {
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Message = request.Message,
                CreatedAt = DateTime.UtcNow
            };

            await _chatCollection.InsertOneAsync(chat);

            // Get sender and receiver info
            var sender = await _userCollection.Find(u => u.Id == senderId).FirstOrDefaultAsync();
            var receiver = await _userCollection.Find(u => u.Id == request.ReceiverId).FirstOrDefaultAsync();

            return new ChatMessageDto
            {
                Id = chat.Id!,
                SenderId = chat.SenderId,
                SenderName = sender?.FullName ?? "Unknown",
                SenderAvatar = sender?.AvatarUrl ?? "",
                ReceiverId = chat.ReceiverId,
                ReceiverName = receiver?.FullName ?? "Unknown",
                ReceiverAvatar = receiver?.AvatarUrl ?? "",
                Message = chat.Message,
                CreatedAt = chat.CreatedAt,
                IsSentByCurrentUser = true
            };
        }

        public async Task<List<ChatHistoryDto>> GetChatHistoryAsync(string userId)
        {
            var filter = Builders<Chat>.Filter.Or(
                Builders<Chat>.Filter.Eq(c => c.SenderId, userId),
                Builders<Chat>.Filter.Eq(c => c.ReceiverId, userId)
            );

            var chats = await _chatCollection.Find(filter).ToListAsync();
            var users = await _userCollection.Find(_ => true).ToListAsync();

            var chatGroups = chats
                .GroupBy(c => c.SenderId == userId ? c.ReceiverId : c.SenderId)
                .Select(g => new ChatHistoryDto
                {
                    ParticipantId = g.Key,
                    ParticipantName = users.FirstOrDefault(u => u.Id == g.Key)?.FullName ?? "Unknown",
                    ParticipantAvatar = users.FirstOrDefault(u => u.Id == g.Key)?.AvatarUrl ?? "",
                    LastMessage = g.OrderByDescending(c => c.CreatedAt).First().Message,
                    LastMessageTime = g.OrderByDescending(c => c.CreatedAt).First().CreatedAt
                })
                .OrderByDescending(c => c.LastMessageTime)
                .ToList();

            return chatGroups;
        }

        public async Task<List<ChatMessageDto>> GetChatMessagesAsync(string userId, string otherUserId, int page = 1, int pageSize = 50)
        {
            var filter = Builders<Chat>.Filter.Or(
                Builders<Chat>.Filter.And(
                    Builders<Chat>.Filter.Eq(c => c.SenderId, userId),
                    Builders<Chat>.Filter.Eq(c => c.ReceiverId, otherUserId)
                ),
                Builders<Chat>.Filter.And(
                    Builders<Chat>.Filter.Eq(c => c.SenderId, otherUserId),
                    Builders<Chat>.Filter.Eq(c => c.ReceiverId, userId)
                )
            );

            var sort = Builders<Chat>.Sort.Descending(c => c.CreatedAt);
            var chats = await _chatCollection.Find(filter)
                .Sort(sort)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var userIds = chats.Select(c => c.SenderId).Union(chats.Select(c => c.ReceiverId)).Distinct();
            var userFilter = Builders<User>.Filter.In(u => u.Id, userIds);
            var users = await _userCollection.Find(userFilter).ToListAsync();

            var messages = chats.Select(c =>
            {
                var sender = users.FirstOrDefault(u => u.Id == c.SenderId);
                var receiver = users.FirstOrDefault(u => u.Id == c.ReceiverId);

                return new ChatMessageDto
                {
                    Id = c.Id!,
                    SenderId = c.SenderId,
                    SenderName = sender?.FullName ?? "Unknown",
                    SenderAvatar = sender?.AvatarUrl ?? "",
                    ReceiverId = c.ReceiverId,
                    ReceiverName = receiver?.FullName ?? "Unknown",
                    ReceiverAvatar = receiver?.AvatarUrl ?? "",
                    Message = c.Message,
                    CreatedAt = c.CreatedAt,
                    IsSentByCurrentUser = c.SenderId == userId
                };
            })
            .OrderBy(c => c.CreatedAt)
            .ToList();
            return messages;
        }
    }
}
