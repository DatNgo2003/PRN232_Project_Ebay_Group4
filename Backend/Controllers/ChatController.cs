using Backend.Models;
using Backend.Repositories;
using Backend.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly ILogger<ChatController> _logger;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<Backend.Hubs.ChatHub> _hubContext;
        private readonly Backend.Models.CloneEbayDbContext _dbContext;

        public ChatController(IMessageRepository messageRepository, ILogger<ChatController> logger, Microsoft.AspNetCore.SignalR.IHubContext<Backend.Hubs.ChatHub> hubContext, Backend.Models.CloneEbayDbContext dbContext)
        {
            _messageRepository = messageRepository;
            _logger = logger;
            _hubContext = hubContext;
            _dbContext = dbContext;
        }

        [HttpPost("messages")]
        public async Task<IActionResult> PostMessage([FromBody] System.Text.Json.JsonElement body)
        {
            try
            {
                // Be tolerant: accept numeric or string values for ids
                int? senderId = null;
                int? receiverId = null;
                string? content = null;

                if (body.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (body.TryGetProperty("senderId", out var s))
                    {
                        if (s.ValueKind == System.Text.Json.JsonValueKind.Number && s.TryGetInt32(out var si)) senderId = si;
                        else if (s.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(s.GetString(), out var sp)) senderId = sp;
                    }
                    if (body.TryGetProperty("receiverId", out var r))
                    {
                        if (r.ValueKind == System.Text.Json.JsonValueKind.Number && r.TryGetInt32(out var ri)) receiverId = ri;
                        else if (r.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(r.GetString(), out var rp)) receiverId = rp;
                    }
                    if (body.TryGetProperty("content", out var c))
                    {
                        if (c.ValueKind == System.Text.Json.JsonValueKind.String) content = c.GetString();
                    }
                }

                if (senderId == null || receiverId == null || string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("ChatController: invalid payload. sender={Sender} receiver={Receiver} contentEmpty={Empty}", senderId, receiverId, string.IsNullOrWhiteSpace(content));
                    return BadRequest("Invalid message payload: senderId, receiverId and content are required.");
                }

                var msg = new Message
                {
                    SenderId = senderId.Value,
                    ReceiverId = receiverId.Value,
                    Content = content,
                    Timestamp = DateTime.UtcNow
                };

                var saved = await _messageRepository.AddMessageAsync(msg);
                // build DTO including sender username
                var senderUser = await _dbContext.Users.FindAsync(saved.SenderId);
                var res = new ChatMessageDto
                {
                    Id = saved.Id,
                    SenderId = saved.SenderId,
                    ReceiverId = saved.ReceiverId,
                    Content = saved.Content,
                    Timestamp = saved.Timestamp.ToString("o"),
                    SenderUsername = senderUser?.Username
                };

                // Broadcast to any connected clients for sender and receiver (so REST fallback also updates UIs realtime)
                var senderConns = Backend.Hubs.ChatHub.GetConnectionsForUser(saved.SenderId);
                var receiverConns = Backend.Hubs.ChatHub.GetConnectionsForUser(saved.ReceiverId);
                try
                {
                    if (senderConns != null && senderConns.Count > 0)
                    {
                        await _hubContext.Clients.Clients(senderConns).SendAsync("ReceiveMessage", res);
                    }
                    if (receiverConns != null && receiverConns.Count > 0)
                    {
                        await _hubContext.Clients.Clients(receiverConns).SendAsync("ReceiveMessage", res);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ChatController: broadcasting saved message via hub failed");
                }

                return Ok(res);
            }
            catch (Exception ex)
            {
                // Log full exception and include inner exception message in response for debugging
                _logger.LogError(ex, "ChatController: failed to save message (detailed)");
                var inner = ex.InnerException?.Message ?? ex.GetBaseException()?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Server error while saving message", detail = inner });
            }
        }

        [HttpGet("conversation")]
        public async Task<IActionResult> GetConversation([FromQuery] int userAId, [FromQuery] int userBId)
        {
            var msgs = await _messageRepository.GetConversationAsync(userAId, userBId);
            var res = msgs.Select(m => new ChatMessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                Timestamp = m.Timestamp.ToString("o"),
                SenderUsername = null
            });
            return Ok(res);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetChatUsers([FromQuery] int adminId = 1)
        {
            var users = await _messageRepository.GetChatUsersAsync(adminId);
            return Ok(users);
        }
    }
}
