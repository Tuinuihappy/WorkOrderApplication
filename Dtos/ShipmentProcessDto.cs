using System;

namespace WorkOrderApplication.API.Dtos
{
    /// <summary>
    /// DTO สำหรับข้อมูล ShipmentProcess (ใช้ส่ง/รับข้อมูลจาก API หรือ SignalR)
    /// </summary>
    public record ShipmentProcessDto(
        int Id,
        int SourceStationId,
        string SourceStation,
        int DestinationStationId,
        string DestinationStation,
        int OrderGroupId,
        int ExternalId,
        string OrderId,
        string OrderName,
        int? OrderState,
        int? ExecutingIndex,
        double? Progress,
        string? ExecuteVehicleName,
        string? ExecuteVehicleKey,
        DateTime? LastSynced,
        int OrderProcessId,
        string? OrderProcessName,
        DateTime? ArrivalTime
    );
}

public record ShipmentArrivalDto(
    DateTime ArrivalTime
);