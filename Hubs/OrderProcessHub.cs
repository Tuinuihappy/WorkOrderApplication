using Microsoft.AspNetCore.SignalR;

namespace WorkOrderApplication.API.Hubs;
using WorkOrderApplication.API.Constants;

public class OrderProcessHub : Hub<IOrderClient>
{
    private static int _connectedClients = 0;
    private readonly ILogger<OrderProcessHub> _logger;

    public OrderProcessHub(ILogger<OrderProcessHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        Interlocked.Increment(ref _connectedClients);
        _logger.LogInformation("✅ Client connected: {ConnectionId} | Total: {Count}",
            Context.ConnectionId, _connectedClients);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Interlocked.Decrement(ref _connectedClients);
        _logger.LogInformation("❌ Client disconnected: {ConnectionId} | Total: {Count}",
            Context.ConnectionId, _connectedClients);

        await base.OnDisconnectedAsync(exception);
    }

    // ✅ สำหรับหน้า List ทั้งหมด
    public async Task JoinOrderList()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroups.AllOrders);
        _logger.LogInformation("👥 Client {ConnectionId} joined group '{Group}'", Context.ConnectionId, SignalRGroups.AllOrders);
    }

    public async Task LeaveOrderList()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRGroups.AllOrders);
        _logger.LogInformation("🚪 Client {ConnectionId} left group '{Group}'", Context.ConnectionId, SignalRGroups.AllOrders);
    }

    // ✅ สำหรับหน้า Details ของแต่ละ order
    public async Task JoinOrderDetails(int orderProcessId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroups.OrderDetails(orderProcessId));
        _logger.LogInformation("🔍 Client {ConnectionId} joined group '{Group}'", Context.ConnectionId, SignalRGroups.OrderDetails(orderProcessId));
    }

    public async Task LeaveOrderDetails(int orderProcessId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRGroups.OrderDetails(orderProcessId));
        _logger.LogInformation("🚪 Client {ConnectionId} left group '{Group}'", Context.ConnectionId, SignalRGroups.OrderDetails(orderProcessId));
    }
}
