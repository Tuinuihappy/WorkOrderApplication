using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Hubs;

public interface IOrderClient
{
    // ---------------- Order Process ----------------
    Task OrderProcessCreated(OrderProcessDetailsDto dto);
    Task OrderProcessUpdated(OrderProcessDetailsDto dto);
    Task OrderProcessDeleted(int orderProcessId);

    // ---------------- Confirm Process ----------------
    Task ConfirmCreated(ConfirmProcessDetailsDto dto);
    Task ConfirmUpdated(ConfirmProcessDetailsDto dto);
    Task ConfirmDeleted(int confirmId);

    // ---------------- Preparing Process ----------------
    Task PreparingCreated(PreparingProcessDetailsDto dto);
    Task PreparingUpdated(PreparingProcessDetailsDto dto);
    Task PreparingDeleted(int preparingId);

    // ---------------- Shipment Process ----------------
    Task ShipmentCreated(ShipmentProcessDto dto);
    Task ShipmentUpdated(ShipmentProcessDto dto);
    Task ShipmentArrived(ShipmentProcessDto dto);
    Task ShipmentDeleted(int shipmentId);
    Task ShipmentStateChanged(ShipmentProcessDto dto);

    // ---------------- Received Process ----------------
    Task ReceivedCreated(ReceivedProcessDetailsDto dto);
    Task ReceivedUpdated(ReceivedProcessDetailsDto dto);
    Task ReceivedDeleted(int receivedId);

    // ---------------- Background Sync ----------------
    Task OrderRecordUpdated(IEnumerable<OrderRecordDto> records);
    Task OrderMissionsUpdated(IEnumerable<OrderMission> missions);
}
