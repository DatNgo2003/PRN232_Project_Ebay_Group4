using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly CloneEbayDbContext _context;

        public ReviewRepository(CloneEbayDbContext context)
        {
            _context = context;
        }

        public async Task AddReviewAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasUserReviewedProductAsync(int reviewerId, int productId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.ProductId == productId && r.ReviewerId == reviewerId);
        }
    }
}