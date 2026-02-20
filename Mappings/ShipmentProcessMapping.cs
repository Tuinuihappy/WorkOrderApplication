using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Extensions;

namespace WorkOrderApplication.API.Mappings
{
    public static class ShipmentProcessMapping
    {
        // -------------------- Entity → DetailsDto --------------------
        public static ShipmentProcessDetailsDto ToDetailsDto(this ShipmentProcess entity)
            => new(
            Id: entity.Id,
            ShipmentMode: entity.ShipmentMode.ToString(),
            SourceStationId: entity.SourceStationId,
            SourceStation: entity.SourceStation,
            DestinationStationId: entity.DestinationStationId,
            DestinationStation: entity.DestinationStation,
            OrderGroupId: entity.OrderGroupId,
            ExternalId: entity.ExternalId,
            OrderId: entity.OrderId,
            OrderName: entity.OrderName,
            OrderState: entity.OrderState,
            ExecutingIndex: entity.ExecutingIndex,
            Progress: entity.Progress,
            ExecuteVehicleName: entity.ExecuteVehicleName,
            ExecuteVehicleKey: entity.ExecuteVehicleKey,
            LastSynced: entity.LastSynced,
            ArrivalTime: entity.ArrivalTime.ToICT(),
            OrderProcessId: entity.OrderProcessId,
            OrderProcessOrderNumber: entity.OrderProcess?.OrderNumber
            );

        // -------------------- Entity → ListDto --------------------
        public static ShipmentProcessListDto ToListDto(this ShipmentProcess entity)
            => new(
            Id: entity.Id,
            ShipmentMode: entity.ShipmentMode.ToString(),
            SourceStation: entity.SourceStation,
            DestinationStation: entity.DestinationStation,
            ExecuteVehicleName: entity.ExecuteVehicleName,
            ArrivalTime: entity.ArrivalTime.ToICT(),
            OrderProcessId: entity.OrderProcessId,
            OrderProcessOrderNumber: entity.OrderProcess?.OrderNumber
            );

        // -------------------- Entity → Dto (เดิม - ใช้สำหรับ SignalR) --------------------
        public static ShipmentProcessDto ToDto(this ShipmentProcess entity)
            => new(
            Id: entity.Id,
            SourceStationId: entity.SourceStationId,
            SourceStation: entity.SourceStation,
            DestinationStationId: entity.DestinationStationId,
            DestinationStation: entity.DestinationStation,
            OrderGroupId: entity.OrderGroupId,
            ExternalId: entity.ExternalId,
            OrderId: entity.OrderId,
            OrderName: entity.OrderName,
            OrderState: entity.OrderState,
            ExecutingIndex: entity.ExecutingIndex,
            Progress: entity.Progress,
            ExecuteVehicleName: entity.ExecuteVehicleName,
            ExecuteVehicleKey: entity.ExecuteVehicleKey,
            LastSynced: entity.LastSynced,
            OrderProcessId: entity.OrderProcessId,
            OrderProcessName: entity.OrderProcess?.ToString(),
            ShipmentMode: entity.ShipmentMode.ToString(),
            ArrivalTime: entity.ArrivalTime.ToICT()
            );

        // -------------------- Dto → Entity --------------------
        public static ShipmentProcess ToEntity(this ShipmentProcessDto dto)
        => new()
        {
            Id = dto.Id,
            SourceStationId = dto.SourceStationId,
            SourceStation = dto.SourceStation,
            DestinationStationId = dto.DestinationStationId,
            DestinationStation = dto.DestinationStation,
            OrderGroupId = dto.OrderGroupId,
            ExternalId = dto.ExternalId,
            OrderId = dto.OrderId,
            OrderName = dto.OrderName,
            OrderState = dto.OrderState,
            ExecutingIndex = dto.ExecutingIndex,
            Progress = dto.Progress,
            ExecuteVehicleName = dto.ExecuteVehicleName,
            ExecuteVehicleKey = dto.ExecuteVehicleKey,
            LastSynced = dto.LastSynced,
            OrderProcessId = dto.OrderProcessId,
            ArrivalTime = dto.ArrivalTime
        };

    }
}
