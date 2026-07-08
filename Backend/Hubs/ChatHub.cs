using Backend.DTOs.Responses;
using Backend.Models;
using Backend.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs
{
    public class ChatHub : Hub
    {
    private readonly IMessageRepository _messageRepository;
    private readonly CloneEbayDbContext _dbContext;
        private static readonly Dictionary<int, HashSet<string>> _connections = new();
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IMessageRepository messageRepository, CloneEbayDbContext dbContext, ILogger<ChatHub> logger)
        {
            _messageRepository = messageRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("ChatHub: connection established {ConnectionId}", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // remove connection from any user mappings
            foreach (var kv in _connections)
            {
                if (kv.Value.Remove(Context.ConnectionId))
                {
                    _logger.LogInformation("ChatHub: removed connection {ConnectionId} from user {UserId}", Context.ConnectionId, kv.Key);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Helper to retrieve current connection ids for a user (used by controllers to broadcast when messages are saved via REST)
        public static IReadOnlyList<string> GetConnectionsForUser(int userId)
        {
            if (_connections.TryGetValue(userId, out var conns))
            {
                lock (conns)
                {
                    return conns.ToList();
                }
            }
            return Array.Empty<string>();
        }

        // client should call Register after connecting to associate current user id
        public Task Register(int userId)
        {
            lock (_connections)
            {
                if (!_connections.ContainsKey(userId)) _connections[userId] = new HashSet<string>();
                _connections[userId].Add(Context.ConnectionId);
            }
            _logger.LogInformation("ChatHub: registered user {UserId} to connection {ConnectionId}", userId, Context.ConnectionId);
            return Task.CompletedTask;
        }

        public async Task SendMessage(int senderId, int receiverId, string content)
        {
            _logger.LogInformation("ChatHub: SendMessage from {Sender} to {Receiver}: {Content}", senderId, receiverId, content);

            try
            {
                var msg = new Message
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Content = content,
                    Timestamp = DateTime.UtcNow
                };

                var saved = await _messageRepository.AddMessageAsync(msg);

                // Try to resolve sender username for client display
                var senderUser = await _dbContext.Users.FindAsync(saved.SenderId);

                var dto = new ChatMessageDto
                {
                    Id = saved.Id,
                    SenderId = saved.SenderId,
                    ReceiverId = saved.ReceiverId,
                    Content = saved.Content,
                    Timestamp = saved.Timestamp.ToString("o"),
                    SenderUsername = senderUser?.Username
                };

                // send to sender connections
                if (_connections.TryGetValue(senderId, out var senderConns))
                {
                    await Clients.Clients(senderConns.ToList()).SendAsync("ReceiveMessage", dto);
                }

                // send to receiver connections
                if (_connections.TryGetValue(receiverId, out var receiverConns))
                {
                    await Clients.Clients(receiverConns.ToList()).SendAsync("ReceiveMessage", dto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChatHub: failed to save/send message from {Sender} to {Receiver}", senderId, receiverId);
                // notify caller of failure so client can fallback
                await Clients.Caller.SendAsync("SendFailed", ex.Message);
            }
        }

        public async Task<IEnumerable<ChatMessageDto>> GetConversation(int userAId, int userBId)
        {
            var msgs = await _messageRepository.GetConversationAsync(userAId, userBId);
            var dtos = new List<ChatMessageDto>();
            foreach(var m in msgs)
            {
                var su = await _dbContext.Users.FindAsync(m.SenderId);
                dtos.Add(new ChatMessageDto {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    Timestamp = m.Timestamp.ToString("o"),
                    SenderUsername = su?.Username
                });
            }

            return dtos;
        }
    }
}
