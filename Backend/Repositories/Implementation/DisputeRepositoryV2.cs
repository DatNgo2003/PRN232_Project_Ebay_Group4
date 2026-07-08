using Backend.Models;
using Backend.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Implementation
{
    public class DisputeRepositoryV2 : IDisputeRepositoryV2
    {
        private readonly CloneEbayDbContext _context;

        public DisputeRepositoryV2(CloneEbayDbContext context)
        {
            _context = context;
        }

        public async Task<OrderTable> GetOrderByIdAndBuyerAsync(int orderId, int buyerId)
        {
            return await _context.OrderTables
                .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerId == buyerId);
        }

        public async Task<IEnumerable<OrderTable>> GetDisputableOrdersAsync(int buyerId)
        {
            var disputableStatuses = new[] { "Completed", "Shipped" };

            return await _context.OrderTables
                .Where(o => o.BuyerId == buyerId && disputableStatuses.Contains(o.Status))
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Seller)
                            .ThenInclude(s => s.Stores)
                .Where(o => !_context.Disputes.Any(d => d.OrderId == o.Id && d.Status == "1"))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // === Dispute Methods ===
        public async Task<Dispute> AddDisputeAsync(Dispute dispute)
        {
            dispute.Status = "1";
            await _context.Disputes.AddAsync(dispute);
            
            await _context.SaveChangesAsync(); // Giả định lưu ngay lập tức
            return dispute;
        }

        public async Task<bool> HasPendingDisputeAsync(int orderId)
        {
            return await _context.Disputes
                .AnyAsync(d => d.OrderId == orderId && d.Status == "1");
        }
    }    
}
