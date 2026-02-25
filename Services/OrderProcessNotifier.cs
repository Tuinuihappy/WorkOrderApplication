using Microsoft.AspNetCore.SignalR;
using WorkOrderApplication.API.Constants;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Hubs;
using WorkOrderApplication.API.Mappings;

namespace WorkOrderApplication.API.Services;

public class OrderProcessNotifier
{
    private readonly IHubContext<OrderProcessHub, IOrderClient> _hub;
    private readonly ILogger<OrderProcessNotifier> _logger;

    public OrderProcessNotifier(
        IHubContext<OrderProcessHub, IOrderClient> hub,
        ILogger<OrderProcessNotifier> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    // --------------------------------------- OrderProcess Notifications ---------------------------------------
    // ✅ Broadcast เมื่อมีการสร้าง OrderProcess ใหม่
    public async Task BroadcastCreatedAsync(int orderProcessId, OrderProcessDetailsDto dto)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.AllOrders)
                .OrderProcessCreated(dto);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", SignalREvents.OrderProcessCreated, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.OrderProcessCreated, orderProcessId);
        }
    }

    // ✅ Broadcast เมื่อมีการอัปเดต OrderProcess
    public async Task BroadcastUpdatedAsync(int orderProcessId, OrderProcessDetailsDto dto)
    {
        try
        {
            // ส่งให้หน้า Details ของ order นั้น
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .OrderProcessUpdated(dto);

            // ส่งให้หน้า List ด้วย (ถ้ามี)
            await _hub.Clients.Group(SignalRGroups.AllOrders)
                .OrderProcessUpdated(dto);

            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", SignalREvents.OrderProcessUpdated, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.OrderProcessUpdated, orderProcessId);
        }
    }

    // ✅ Broadcast เมื่อมีการลบ OrderProcess
    public async Task BroadcastDeletedAsync(int orderProcessId)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.AllOrders)
                .OrderProcessDeleted(orderProcessId);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", SignalREvents.OrderProcessDeleted, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.OrderProcessDeleted, orderProcessId);
        }
    }


    // --------------------------------------- Confirm Process Notifications ---------------------------------------
    // ✅ Confirm Created
    public async Task BroadcastConfirmCreatedAsync(int orderProcessId, ConfirmProcessDetailsDto dto)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .ConfirmCreated(dto);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", SignalREvents.ConfirmCreated, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.ConfirmCreated, orderProcessId);
        }
    }

    // ✅ Confirm Updated
    public async Task BroadcastConfirmUpdatedAsync(int orderProcessId, ConfirmProcessDetailsDto dto)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .ConfirmUpdated(dto);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", SignalREvents.ConfirmUpdated, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.ConfirmUpdated, orderProcessId);
        }
    }

    // ✅ Confirm Deleted
    public async Task BroadcastConfirmDeletedAsync(int orderProcessId, int confirmId)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .ConfirmDeleted(confirmId);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", SignalREvents.ConfirmDeleted, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.ConfirmDeleted, orderProcessId);
        }
    }

    // --------------------------------------- Preparing Process Notifications ---------------------------------------
    // ✅ Preparing Created
    public async Task BroadcastPreparingCreatedAsync(int orderProcessId, PreparingProcessDetailsDto dto)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .PreparingCreated(dto);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", SignalREvents.PreparingCreated, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.PreparingCreated, orderProcessId);
        }
    }

    // ✅ Preparing Updated
    public async Task BroadcastPreparingUpdatedAsync(int orderProcessId, PreparingProcessDetailsDto dto)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .PreparingUpdated(dto);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", SignalREvents.PreparingUpdated, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.PreparingUpdated, orderProcessId);
        }
    }

    // ✅ Preparing Deleted
    public async Task BroadcastPreparingDeletedAsync(int orderProcessId, int preparingId)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .PreparingDeleted(preparingId);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", SignalREvents.PreparingDeleted, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.PreparingDeleted, orderProcessId);
        }
    }

    // --------------------------------------- Shipping Process Notifications ---------------------------------------
    public async Task BroadcastShipmentCreatedAsync(int orderProcessId, ShipmentProcessDto dto)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .ShipmentCreated(dto);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", SignalREvents.ShipmentCreated, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.ShipmentCreated, orderProcessId);
        }
    }

    public async Task BroadcastShipmentUpdatedAsync(int orderProcessId, ShipmentProcessDto dto)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .ShipmentUpdated(dto);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", SignalREvents.ShipmentUpdated, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.ShipmentUpdated, orderProcessId);
        }
    }

    public async Task BroadcastShipmentArrivedAsync(int orderProcessId, ShipmentProcessDto dto)
    {
        try
        {
            // ส่งให้หน้ารายละเอียดของ order นั้น (ทุก client ที่ join group order-{id})
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .ShipmentArrived(dto);
            _logger.LogInformation("📦 {Event} broadcasted for OrderProcessId={OrderProcessId}, ShipmentId={ShipmentId}",
                SignalREvents.ShipmentArrived, orderProcessId, dto.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.ShipmentArrived, orderProcessId);
        }
    }

    public async Task BroadcastShipmentDeletedAsync(int orderProcessId, int shipmentId)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .ShipmentDeleted(shipmentId);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId {Id}", SignalREvents.ShipmentDeleted, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.ShipmentDeleted, orderProcessId);
        }
    }

    /// <summary>
    /// ใช้ใน BackgroundService เมื่อข้อมูล orderRecordById เปลี่ยน
    /// </summary>
    public async Task BroadcastShipmentStateChangedAsync(ShipmentProcess shipment)
    {
        try
        {
            var dto = shipment.ToDto(); // ✅ ใช้ Mapping เดิมของคุณ
            await _hub.Clients.Group(SignalRGroups.OrderDetails(shipment.OrderProcessId))
                .ShipmentStateChanged(dto);
            _logger.LogInformation("📡 [Background] Shipment state changed: {OrderName} ({ExternalId})",
                shipment.OrderName, shipment.ExternalId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.ShipmentStateChanged, shipment.OrderProcessId);
        }
    }

    // --------------------------------------- Received Process Notifications ---------------------------------------
    public async Task BroadcastReceivedCreatedAsync(int orderProcessId, ReceivedProcessDetailsDto dto)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .ReceivedCreated(dto);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId={Id}", SignalREvents.ReceivedCreated, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.ReceivedCreated, orderProcessId);
        }
    }

    public async Task BroadcastReceivedUpdatedAsync(int orderProcessId, ReceivedProcessDetailsDto dto)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .ReceivedUpdated(dto);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId={Id}", SignalREvents.ReceivedUpdated, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.ReceivedUpdated, orderProcessId);
        }
    }

    public async Task BroadcastReceivedDeletedAsync(int orderProcessId, int receivedId)
    {
        try
        {
            await _hub.Clients.Group(SignalRGroups.OrderDetails(orderProcessId))
                .ReceivedDeleted(receivedId);
            _logger.LogInformation("📢 Broadcast {Event} for OrderProcessId={Id}", SignalREvents.ReceivedDeleted, orderProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to broadcast {Event} for OrderProcessId {Id}", SignalREvents.ReceivedDeleted, orderProcessId);
        }
    }


}
