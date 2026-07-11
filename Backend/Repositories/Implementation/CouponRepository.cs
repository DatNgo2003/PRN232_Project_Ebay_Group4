using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class CouponRepository : ICouponRepository
    {
        private readonly CloneEbayDbContext _context;

        public CouponRepository(CloneEbayDbContext context)
        {
            _context = context;
        }

        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            return await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code);
        }

        public async Task IncrementUsedCountAsync(int couponId)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == couponId);
            if (coupon == null) return;

            coupon.UsedCount = (coupon.UsedCount ?? 0) + 1;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Coupon>> GetAvailableForProductAsync(int productId)
        {
            var now = DateTime.UtcNow;

            return await _context.Coupons
                .Where(c =>
                    (c.ProductId == null || c.ProductId == productId) &&
                    (c.StartDate == null || c.StartDate <= now) &&
                    (c.EndDate == null || c.EndDate >= now) &&
                    (c.DiscountPercent != null && c.DiscountPercent > 0) &&
                    (c.MaxUsage == null || (c.UsedCount ?? 0) < c.MaxUsage))
                .OrderByDescending(c => c.DiscountPercent)
                .ToListAsync();
        }
    }
}