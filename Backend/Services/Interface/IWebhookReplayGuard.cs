using System;
using System.Threading.Tasks;

namespace Backend.Services
{
    public interface IWebhookReplayGuard
    {
        Task<bool> TryMarkProcessedAsync(string eventKey, DateTimeOffset eventTime);
    }
}