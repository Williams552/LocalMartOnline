using Microsoft.AspNetCore.SignalR;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Models.DTOs.Chat;

namespace LocalMartOnline.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private static readonly Dictionary<string, string> UserConnections = new();

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task JoinChat(string userId)
        {
            UserConnections[userId] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public async Task LeaveChat(string userId)
        {
            UserConnections.Remove(userId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            var request = new SendMessageRequestDto
            {
                ReceiverId = receiverId,
                Message = message
            };

            var chatMessage = await _chatService.SendMessageAsync(senderId, request);

            // Send to receiver
            if (UserConnections.ContainsKey(receiverId))
            {
                await Clients.Group($"User_{receiverId}").SendAsync("ReceiveMessage", chatMessage);
            }

            // Send back to sender
            await Clients.Group($"User_{senderId}").SendAsync("MessageSent", chatMessage);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections.Remove(userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
