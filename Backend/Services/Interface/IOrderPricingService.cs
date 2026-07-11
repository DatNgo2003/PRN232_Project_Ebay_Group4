using System.Threading.Tasks;
using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services
{
    public interface IOrderPricingService
    {
        Task<OrderPricingResultDto> CalculateAsync(CalculateOrderDto request);
    }
}