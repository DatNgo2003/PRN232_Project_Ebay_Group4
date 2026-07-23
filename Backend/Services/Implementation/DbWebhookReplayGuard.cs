using System;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public sealed class DbWebhookReplayGuard : IWebhookReplayGuard
    {
        private readonly CloneEbayDbContext _context;

        public DbWebhookReplayGuard(CloneEbayDbContext context)
        {
            _context = context;
        }

        public async Task<bool> TryMarkProcessedAsync(string eventKey, DateTimeOffset eventTime)
        {
            var exists = await _context.ProcessedWebhookEvents
                .AnyAsync(e => e.EventKey == eventKey);
            if (exists) return false;

            try
            {
                _context.ProcessedWebhookEvents.Add(new ProcessedWebhookEvent
                {
                    EventKey = eventKey,
                    ProcessedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                // Vi phạm unique constraint = 2 request trùng chữ ký xử lý cùng lúc
                return false;
            }
        }
    }
}