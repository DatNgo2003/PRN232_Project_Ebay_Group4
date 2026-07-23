using Backend.Services.Interface;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Services;

public sealed class InMemoryWebhookReplayGuard : IWebhookReplayGuard
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan RetentionWindow = TimeSpan.FromMinutes(10);

    public InMemoryWebhookReplayGuard(IMemoryCache cache) => _cache = cache;

    public Task<bool> TryMarkProcessedAsync(string eventKey, DateTimeOffset eventTime)
    {
        if (_cache.TryGetValue(eventKey, out _))
            return Task.FromResult(false); // đã thấy rồi -> replay

        _cache.Set(eventKey, true, RetentionWindow);
        return Task.FromResult(true);
    }
}