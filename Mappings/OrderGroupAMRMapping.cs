using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Mappings;

public static class OrderGroupAMRMapping
{
    // 🟢 Mapping: UpsertDto → Entity
    public static OrderGroupAMR ToEntity(this OrderGroupAMRUpsertDto dto)
    {
        return new OrderGroupAMR
        {
            SourceStationId = dto.SourceStationId,
            SourceStation = dto.SourceStation,
            DestinationStationId = dto.DestinationStationId,
            DestinationStation = dto.DestinationStation,
            OrderGroupId = dto.OrderGroupId
        };
    }

    // 🟡 Mapping: Entity → DetailsDto
    public static OrderGroupAMRDetailsDto ToDetailsDto(this OrderGroupAMR entity)
    {
        return new OrderGroupAMRDetailsDto(
            entity.Id,
            entity.SourceStationId,
            entity.SourceStation,
            entity.DestinationStationId,
            entity.DestinationStation,
            entity.OrderGroupId
        );
    }

    // 🔵 Mapping: Entity → ListDto
    public static OrderGroupAMRListDto ToListDto(this OrderGroupAMR entity)
    {
        return new OrderGroupAMRListDto(
            entity.Id,
            entity.SourceStation,
            entity.DestinationStation,
            entity.OrderGroupId
        );
    }

    // 🟠 Mapping: Update existing entity (ใช้ตอน PUT/PATCH)
    public static void UpdateEntity(this OrderGroupAMR entity, OrderGroupAMRUpsertDto dto)
    {
        entity.SourceStationId = dto.SourceStationId;
        entity.SourceStation = dto.SourceStation;
        entity.DestinationStationId = dto.DestinationStationId;
        entity.DestinationStation = dto.DestinationStation;
        entity.OrderGroupId = dto.OrderGroupId;
    }
}
