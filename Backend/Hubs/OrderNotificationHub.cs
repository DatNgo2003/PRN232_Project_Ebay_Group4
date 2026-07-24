using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs;

public class OrderNotificationHub : Hub
{
    private readonly ILogger<OrderNotificationHub> _logger;

    public OrderNotificationHub(ILogger<OrderNotificationHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("OrderNotificationHub: connection established {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("OrderNotificationHub: connection closed {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task JoinUserGroup(int userId)
    {
        var groupName = $"User_{userId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("OrderNotificationHub: Connection {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task JoinOrderGroup(int orderId)
    {
        var groupName = $"Order_{orderId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("OrderNotificationHub: Connection {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }
}
