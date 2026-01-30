using Microsoft.AspNetCore.SignalR;

namespace WorkOrderApplication.API.Hubs;

public class OrderProcessHub : Hub
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
        await Groups.AddToGroupAsync(Context.ConnectionId, "orders-all");
        _logger.LogInformation("👥 Client {ConnectionId} joined group 'orders-all'", Context.ConnectionId);
    }

    // ✅ สำหรับหน้า Details ของแต่ละ order
    public async Task JoinOrderDetails(int orderProcessId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderProcessId}");
        _logger.LogInformation("🔍 Client {ConnectionId} joined group 'order-{Id}'", Context.ConnectionId, orderProcessId);
    }

    public async Task LeaveOrderDetails(int orderProcessId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderProcessId}");
        _logger.LogInformation("🚪 Client {ConnectionId} left group 'order-{Id}'", Context.ConnectionId, orderProcessId);
    }
}
