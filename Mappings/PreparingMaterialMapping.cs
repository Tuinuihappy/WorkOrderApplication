using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Mappings;

public static class PreparingMaterialMapping
{
    // -------------------- Entity -> DetailsDto --------------------
    public static PreparingMaterialDetailsDto ToDetailsDto(this PreparingMaterial entity)
    {
        return new PreparingMaterialDetailsDto(
            entity.Id,
            entity.PreparingProcessId,
            entity.MaterialId,
            entity.Material?.MaterialNumber ?? string.Empty,
            entity.Material?.Description ?? string.Empty,
            entity.PreparedQty,
            entity.Material?.Unit ?? string.Empty
        );
    }

    // -------------------- Entity -> ListDto --------------------
    public static PreparingMaterialListDto ToListDto(this PreparingMaterial entity)
    {
        return new PreparingMaterialListDto(
            entity.Id,
            entity.MaterialId,
            entity.Material?.MaterialNumber ?? string.Empty,
            entity.PreparedQty
        );
    }

    // -------------------- UpsertDto -> Entity (Create) --------------------
    public static PreparingMaterial ToEntity(this PreparingMaterialUpsertDto dto)
    {
        return new PreparingMaterial
        {
            MaterialId = dto.MaterialId,
            PreparedQty = dto.PreparedQty
        };
    }

    // -------------------- Update Entity from UpsertDto --------------------
    public static void UpdateEntity(this PreparingMaterial entity, PreparingMaterialUpsertDto dto)
    {
        entity.MaterialId = dto.MaterialId;
        entity.PreparedQty = dto.PreparedQty;

    }
}
