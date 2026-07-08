using Backend.Models;

namespace Backend.Repositories
{
    public interface IMessageRepository
    {
        Task<Message> AddMessageAsync(Message message);
        Task<IEnumerable<Message>> GetConversationAsync(int userAId, int userBId);
        Task<IEnumerable<Backend.DTOs.Responses.ChatUserDto>> GetChatUsersAsync(int adminId);
    }
}
