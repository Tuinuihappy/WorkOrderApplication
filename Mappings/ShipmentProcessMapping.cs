using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Extensions;

namespace WorkOrderApplication.API.Mappings
{
    public static class ShipmentProcessMapping
    {
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
            ShipmentMode: entity.ShipmentMode.ToString(), // ✅ Map Enum to String
            ArrivalTime: entity.ArrivalTime.ToICT()
            );

            
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
