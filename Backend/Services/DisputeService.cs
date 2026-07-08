using Backend.DTOs.Dispute;
using Backend.Repositories;
using Microsoft.Identity.Client;

namespace Backend.Services
{
    public class DisputeService : IDisputeService
    {
        private readonly IDisputeRepository _repository;
        public DisputeService (IDisputeRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DisputeListItemDto>> GetDisputesByBuyerAsync(int buyerId)
        {
            return await _repository.GetDisputesByBuyerAsync (buyerId);
        }

        public async Task<List<DisputeListItemDto>> GetDisputesBySellerAsync(int sellerId)
        {
            return await _repository.GetDisputesBySellerAsync (sellerId);
        }

        public Task<List<DisputeListItemDto>> GetDisputesForSupporterAsync()
        {
            return _repository.GetDisputesForSupporterAsync();
        }

        public void RespondDisputeAsync(RespondDisputeDto respond)
        {
            _repository.RespondDispute (respond);
        }
    }
}
