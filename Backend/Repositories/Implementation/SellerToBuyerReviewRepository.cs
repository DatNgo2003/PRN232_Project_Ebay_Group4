using Backend.Models;
using Backend.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Implementation
{
    public class SellerToBuyerReviewRepository : ISellerToBuyerReviewRepository
    {
        private readonly CloneEbayDbContext _context;

        public SellerToBuyerReviewRepository(CloneEbayDbContext context)
        {
            _context = context;
        }
        public async Task<OrderTable?> GetOrderDetailsAsync(int orderId)
        {
            return await _context.OrderTables
                .Include(o => o.Buyer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> HasBeenReviewedAsync(int orderId, int sellerId)
        {
            return await _context.SellerToBuyerReviews 
                .AnyAsync(r => r.OrderId == orderId && r.SellerId == sellerId);
        }

        public async Task<SellerToBuyerReview> AddReviewAsync(SellerToBuyerReview review)
        {
            _context.SellerToBuyerReviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<IEnumerable<SellerToBuyerReview>> GetReviewsForBuyerAsync(int buyerId)
        {
            return await _context.SellerToBuyerReviews
                .Where(r => r.BuyerId == buyerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
