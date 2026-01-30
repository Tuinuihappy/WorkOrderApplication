using Microsoft.AspNetCore.SignalR;

namespace WorkOrderApplication.API.Hubs;

public class OrderRecordHub : Hub
{
    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"[SignalR] Client connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"[SignalR] Client disconnected: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }
}

