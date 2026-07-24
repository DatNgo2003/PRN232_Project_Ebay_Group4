using Backend.DTOs;

namespace Backend.Services;

public interface IShippingTaskQueue
{
    ValueTask QueueShippingTaskAsync(ShippingTaskMessage message, CancellationToken cancellationToken = default);
    ValueTask<ShippingTaskMessage> DequeueShippingTaskAsync(CancellationToken cancellationToken);
}
