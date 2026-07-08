using Backend.Models;

namespace Backend.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<User?> GetByUsernameAsync(string username);
        Task AddUserAsync(User user);
    }
}