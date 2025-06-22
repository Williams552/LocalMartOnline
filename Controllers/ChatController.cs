using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Services;
using LocalMartOnline.Models.DTOs.Chat;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("send")]
        public async Task<ActionResult<ChatMessageDto>> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                var senderId = request.SenderId; // Trong thực tế, lấy từ JWT token
                var messageDto = new SendMessageRequestDto
                {
                    ReceiverId = request.ReceiverId,
                    Message = request.Message
                };

                var result = await _chatService.SendMessageAsync(senderId, messageDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error sending message: {ex.Message}");
            }
        }

        [HttpGet("history/{userId}")]
        public async Task<ActionResult<List<ChatHistoryDto>>> GetChatHistory(string userId)
        {
            try
            {
                var result = await _chatService.GetChatHistoryAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error getting chat history: {ex.Message}");
            }
        }

        [HttpGet("messages/{userId}/{otherUserId}")]
        public async Task<ActionResult<List<ChatMessageDto>>> GetChatMessages(
            string userId, 
            string otherUserId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var result = await _chatService.GetChatMessagesAsync(userId, otherUserId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error getting chat messages: {ex.Message}");
            }
        }
    }

    // Request models
    public class SendMessageRequest
    {
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
