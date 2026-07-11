using Backend.Models;

namespace Backend.Repositories
{
    public interface ICouponRepository
    {
        Task<Coupon?> GetByCodeAsync(string code);
        Task IncrementUsedCountAsync(int couponId);

        Task<IEnumerable<Coupon>> GetAvailableForProductAsync(int productId);
    }
}