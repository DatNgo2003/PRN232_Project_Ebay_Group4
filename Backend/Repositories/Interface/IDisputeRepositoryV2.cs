using Backend.Models;

namespace Backend.Repositories.Interface
{
    public interface IDisputeRepositoryV2
    {
        Task<OrderTable> GetOrderByIdAndBuyerAsync(int orderId, int buyerId);
        Task<IEnumerable<OrderTable>> GetDisputableOrdersAsync(int buyerId);

        // === Methods liên quan đến Dispute ===
        Task<Dispute> AddDisputeAsync(Dispute dispute);
        Task<bool> HasPendingDisputeAsync(int orderId);

    }




}
