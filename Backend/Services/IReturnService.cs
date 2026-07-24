using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services
{
    public interface IReturnService
    {
        Task<ReturnRequestDto?> CreateReturnRequestAsync(string username, CreateReturnRequestDto dto);
        Task<IEnumerable<ReturnRequestDto>> GetMyReturnRequestsAsync(string username);
        Task<IEnumerable<ReturnRequestDto>> GetReturnRequestsByOrderIdAsync(int orderId);
        Task<IEnumerable<ReturnRequestDto>> GetAllPendingReturnsAsync();
        Task<ReturnRequestDto?> UpdateReturnStatusAsync(int returnId, string status);
        Task<bool> ApproveReturnAsync(int returnId);
        Task<bool> RejectReturnAsync(int returnId);
    }
}
