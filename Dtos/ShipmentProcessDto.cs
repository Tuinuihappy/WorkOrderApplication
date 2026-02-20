using System;
using WorkOrderApplication.API.Enums;

namespace WorkOrderApplication.API.Dtos;

// -------------------- รายละเอียดเต็ม --------------------
public record ShipmentProcessDetailsDto(
    int Id,
    string ShipmentMode,
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
    DateTime? ArrivalTime,
    int OrderProcessId,
    string? OrderProcessOrderNumber // OrderNumber ของ OrderProcess ที่เกี่ยวข้อง
);

// -------------------- สำหรับ List / Table View --------------------
public record ShipmentProcessListDto(
    int Id,
    string ShipmentMode,
    string SourceStation,
    string DestinationStation,
    string? ExecuteVehicleName,
    DateTime? ArrivalTime,
    int OrderProcessId,
    string? OrderProcessOrderNumber
);

// -------------------- DTO เดิม (ใช้สำหรับ SignalR / Backward Compat) --------------------
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
    string? ShipmentMode, // ✅ เพิ่ม ShipmentMode
    DateTime? ArrivalTime
);

public record ShipmentArrivalDto(
    DateTime ArrivalTime
);