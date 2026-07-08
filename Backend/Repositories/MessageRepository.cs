using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly CloneEbayDbContext _context;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(CloneEbayDbContext context, ILogger<MessageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Message> AddMessageAsync(Message message)
        {
            // NOTE: by design we accept plain senderId, receiverId and content and don't enforce that
            // the referenced User rows exist. If the database has FK constraints this may still fail
            // at SaveChanges with a DB exception. To persist without DB-level FKs, remove/drop the
            // foreign key constraints in the database (see instructions in README or below).

            _context.Messages.Add(message);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var inner = dbEx.InnerException?.Message ?? dbEx.GetBaseException()?.Message ?? dbEx.Message;
                _logger.LogError(dbEx, "MessageRepository: DbUpdateException saving message Sender={Sender} Receiver={Receiver}. Inner: {Inner}", message.SenderId, message.ReceiverId, inner);
                // throw a clearer exception with inner details so controller can return useful info during development
                throw new InvalidOperationException("Database update failed: " + inner, dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MessageRepository: SaveChangesAsync failed for message Sender={Sender} Receiver={Receiver}", message.SenderId, message.ReceiverId);
                throw;
            }

            // reload saved entity (do not eager-load navigation to avoid FK joins)
            var saved = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == message.Id);
            return saved ?? message;
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(int userAId, int userBId)
        {
            return await _context.Messages
                .Where(m => (m.SenderId == userAId && m.ReceiverId == userBId) || (m.SenderId == userBId && m.ReceiverId == userAId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<Backend.DTOs.Responses.ChatUserDto>> GetChatUsersAsync(int adminId)
        {
            // Find messages involving admin, group by the other participant id
            var q = await _context.Messages
                .Where(m => m.SenderId == adminId || m.ReceiverId == adminId)
                .Select(m => new {
                    OtherId = (m.SenderId == adminId) ? m.ReceiverId : m.SenderId,
                    m.Content,
                    m.Timestamp
                })
                .GroupBy(x => x.OtherId)
                .Select(g => new {
                    UserId = g.Key,
                    LastMessage = g.OrderByDescending(x => x.Timestamp).FirstOrDefault()
                })
                .ToListAsync();

            var users = q.Select(item => new Backend.DTOs.Responses.ChatUserDto {
                UserId = item.UserId,
                Username = _context.Users.FirstOrDefault(u => u.Id == item.UserId)?.Username,
                LastMessage = item.LastMessage != null ? item.LastMessage.Content : null,
                LastTimestamp = item.LastMessage != null ? item.LastMessage.Timestamp.ToString("o") : null
            }).ToList();

            return users;
        }
    }
}


