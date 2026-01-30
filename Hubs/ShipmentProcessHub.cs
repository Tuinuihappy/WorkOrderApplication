using Microsoft.AspNetCore.SignalR;

namespace WorkOrderApplication.API.Hubs
{
    /// <summary>
    /// SignalR Hub สำหรับ Broadcast ข้อมูล ShipmentProcess แบบ Real-time
    /// </summary>
    public class ShipmentProcessHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"[SignalR] Client connected to ShipmentProcessHub: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"[SignalR] Client disconnected from ShipmentProcessHub: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
