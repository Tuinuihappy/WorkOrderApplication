using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Mappings;

public static class OrderMaterialMapping
{
    // -------------------- Entity → DetailsDto --------------------
    public static OrderMaterialDetailsDto ToDetailsDto(this OrderMaterial entity)
        => new OrderMaterialDetailsDto(
            entity.Id,
            entity.OrderProcessId,
            entity.MaterialId,
            entity.Material?.MaterialNumber ?? string.Empty,
            entity.Material?.MaterialDescription ?? string.Empty,
            entity.OrderQty,
            entity.Material?.BUn ?? string.Empty
        );

    // -------------------- Entity → ListDto --------------------
    public static OrderMaterialListDto ToListDto(this OrderMaterial entity)
        => new OrderMaterialListDto(
            entity.Id,
            entity.MaterialId,
            entity.Material?.MaterialNumber ?? string.Empty,
            entity.OrderQty
        );

    // -------------------- UpsertDto → Entity --------------------
    public static OrderMaterial ToEntity(this OrderMaterialUpsertDto dto)
        => new OrderMaterial
        {
            MaterialId = dto.MaterialId,
            OrderQty = dto.OrderQty
        };

    // -------------------- Update Entity from Dto --------------------
    public static void UpdateEntity(this OrderMaterial entity, OrderMaterialUpsertDto dto)
    {
        entity.MaterialId = dto.MaterialId;
        entity.OrderQty = dto.OrderQty;
    }
}
