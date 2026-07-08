using Backend.DTOs.Requests;

namespace Backend.Services
{
    public interface IReviewService
    {
        Task<(bool Success, string Message)> CreateReviewAsync(CreateReviewDto dto, string reviewerUsername);
    }
}