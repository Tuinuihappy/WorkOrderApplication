using Microsoft.AspNetCore.SignalR;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Hubs;
using WorkOrderApplication.API.Mappings;

namespace WorkOrderApplication.API.Services;

public class OrderProcessNotifier
{
    private readonly IHubContext<OrderProcessHub> _hub;
    private readonly ILogger<OrderProcessNotifier> _logger;

    public OrderProcessNotifier(
        IHubContext<OrderProcessHub> hub,
        ILogger<OrderProcessNotifier> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    // --------------------------------------- OrderProcess Notifications ---------------------------------------
    // ✅ Broadcast เมื่อมีการสร้าง OrderProcess ใหม่
    public async Task BroadcastCreatedAsync(int orderProcessId, OrderProcessDetailsDto dto)
    {
        await _hub.Clients.Group("orders-all")
            .SendAsync("OrderProcessCreated", dto);

        _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", "Created", orderProcessId);
    }

    // ✅ Broadcast เมื่อมีการอัปเดต OrderProcess
    public async Task BroadcastUpdatedAsync(int orderProcessId, OrderProcessDetailsDto dto)
    {
        // ส่งให้หน้า Details ของ order นั้น
        await _hub.Clients.Group($"order-{orderProcessId}")
            .SendAsync("OrderProcessUpdated", dto);

        // ส่งให้หน้า List ด้วย (ถ้ามี)
        await _hub.Clients.Group("orders-all")
            .SendAsync("OrderProcessUpdated", dto);

        _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", "Updated", orderProcessId);
    }

    // ✅ Broadcast เมื่อมีการลบ OrderProcess
    public async Task BroadcastDeletedAsync(int orderProcessId)
    {
        await _hub.Clients.Group("orders-all")
            .SendAsync("OrderProcessDeleted", orderProcessId);

        _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", "Deleted", orderProcessId);
    }


    // --------------------------------------- Confirm Process Notifications ---------------------------------------
    // ✅ Confirm Created
    public async Task BroadcastConfirmCreatedAsync(int orderProcessId, ConfirmProcessDetailsDto dto)
    {
        await _hub.Clients.Group($"order-{orderProcessId}")
            .SendAsync("ConfirmCreated", dto);

        _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", "ConfirmCreated", orderProcessId);
    }

    // ✅ Confirm Updated
    public async Task BroadcastConfirmUpdatedAsync(int orderProcessId, ConfirmProcessDetailsDto dto)
    {
        await _hub.Clients.Group($"order-{orderProcessId}")
            .SendAsync("ConfirmUpdated", dto);

        _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", "ConfirmUpdated", orderProcessId);
    }

    // ✅ Confirm Deleted
    public async Task BroadcastConfirmDeletedAsync(int orderProcessId, int confirmId)
    {
        await _hub.Clients.Group($"order-{orderProcessId}")
            .SendAsync("ConfirmDeleted", confirmId);

        _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", "ConfirmDeleted", orderProcessId);
    }

    // --------------------------------------- Preparing Process Notifications ---------------------------------------
    // ✅ Preparing Created
    public async Task BroadcastPreparingCreatedAsync(int orderProcessId, PreparingProcessDetailsDto dto)
    {
        await _hub.Clients.Group($"order-{orderProcessId}")
            .SendAsync("PreparingCreated", dto);

        _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", "PreparingCreated", orderProcessId);
    }

    // ✅ Preparing Updated
    public async Task BroadcastPreparingUpdatedAsync(int orderProcessId, PreparingProcessDetailsDto dto)
    {
        await _hub.Clients.Group($"order-{orderProcessId}")
            .SendAsync("PreparingUpdated", dto);

        _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", "PreparingUpdated", orderProcessId);
    }

    // ✅ Preparing Deleted
    public async Task BroadcastPreparingDeletedAsync(int orderProcessId, int preparingId)
    {
        await _hub.Clients.Group($"order-{orderProcessId}")
            .SendAsync("PreparingDeleted", preparingId);

        _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", "PreparingDeleted", orderProcessId);
    }

    // --------------------------------------- Shipping Process Notifications ---------------------------------------
    public async Task BroadcastShipmentCreatedAsync(string orderNumber, ShipmentProcessDto dto)
    {
        await _hub.Clients.Group($"order-{dto.OrderProcessId}")
            .SendAsync("ShipmentCreated", dto);
        _logger.LogInformation("📢 Broadcast {Event} for Order {Number}", "ShipmentCreated", orderNumber);
    }

    public async Task BroadcastShipmentUpdatedAsync(string orderNumber, ShipmentProcessDto dto)
    {
        await _hub.Clients.Group($"order-{dto.OrderProcessId}")
            .SendAsync("ShipmentUpdated", dto);
        _logger.LogInformation("📢 Broadcast {Event} for Order {Number}", "ShipmentUpdated", orderNumber);
    }

    public async Task BroadcastShipmentArrivedAsync(string orderNumber, ShipmentProcessDto dto)
    {
        // ส่งให้หน้ารายละเอียดของ order นั้น (ทุก client ที่ join group order-{id})
        await _hub.Clients.Group($"order-{dto.OrderProcessId}")
            .SendAsync("ShipmentArrived", dto);

        _logger.LogInformation("📦 ShipmentArrived broadcasted for Order {OrderNumber}, ShipmentId={ShipmentId}",
            orderNumber, dto.Id);
    }

    public async Task BroadcastShipmentDeletedAsync(string orderNumber, int shipmentId)
    {
        await _hub.Clients.Group($"order-{orderNumber}")
            .SendAsync("ShipmentDeleted", shipmentId);
        _logger.LogInformation("📢 Broadcast {Event} for Order {Number}", "ShipmentDeleted", orderNumber);
    }

    /// <summary>
    /// ใช้ใน BackgroundService เมื่อข้อมูล orderRecordById เปลี่ยน
    /// </summary>
    public async Task BroadcastShipmentStateChangedAsync(ShipmentProcess shipment)
    {
        var dto = shipment.ToDto(); // ✅ ใช้ Mapping เดิมของคุณ
        await _hub.Clients.Group($"order-{shipment.OrderProcessId}")
            .SendAsync("ShipmentStateChanged", dto);
        _logger.LogInformation("📡 [Background] Shipment state changed: {OrderName} ({ExternalId})",
            shipment.OrderName, shipment.ExternalId);
    }

    // --------------------------------------- Received Process Notifications ---------------------------------------
    public async Task BroadcastReceivedCreatedAsync(int orderProcessId, ReceivedProcessDetailsDto dto)
    {
        await _hub.Clients.Group($"order-{orderProcessId}")
            .SendAsync("ReceivedCreated", dto);

        _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId={Id}", "ReceivedCreated", orderProcessId);
    }

    public async Task BroadcastReceivedUpdatedAsync(int orderProcessId, ReceivedProcessDetailsDto dto)
    {
        await _hub.Clients.Group($"order-{orderProcessId}")
            .SendAsync("ReceivedUpdated", dto);

        _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId={Id}", "ReceivedUpdated", orderProcessId);
    }

    public async Task BroadcastReceivedDeletedAsync(int orderProcessId, int receivedId)
    {
        await _hub.Clients.Group($"order-{orderProcessId}")
            .SendAsync("ReceivedDeleted", receivedId);

        _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId={Id}", "ReceivedDeleted", orderProcessId);
    }


}
