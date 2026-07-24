using Backend.DTOs;
using System.Threading.Channels;

namespace Backend.Services.Implementation;

public sealed class ShippingTaskQueue : IShippingTaskQueue
{
    private readonly Channel<ShippingTaskMessage> _channel;

    public ShippingTaskQueue(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<ShippingTaskMessage>(options);
    }

    public async ValueTask QueueShippingTaskAsync(ShippingTaskMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        await _channel.Writer.WriteAsync(message, cancellationToken);
    }

    public async ValueTask<ShippingTaskMessage> DequeueShippingTaskAsync(CancellationToken cancellationToken)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }
}
