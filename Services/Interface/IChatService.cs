using LocalMartOnline.Models.DTOs.Chat;

namespace LocalMartOnline.Services.Interface
{
    public interface IChatService
    {
        Task<ChatMessageDto> SendMessageAsync(string senderId, SendMessageRequestDto request);
        Task<List<ChatHistoryDto>> GetChatHistoryAsync(string userId);
        Task<List<ChatMessageDto>> GetChatMessagesAsync(string userId, string otherUserId, int page = 1, int pageSize = 50);
    }
}
