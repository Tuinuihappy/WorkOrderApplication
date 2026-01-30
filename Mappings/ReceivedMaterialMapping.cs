using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Mappings;

public static class ReceivedMaterialMapping
{
    // -------------------- Entity -> DetailsDto --------------------
    public static ReceivedMaterialDetailsDto ToDetailsDto(this ReceivedMaterial entity)
    {
        return new ReceivedMaterialDetailsDto(
            entity.Id,
            entity.ReceivedProcessId,
            entity.MaterialId,
            entity.Material?.MaterialNumber ?? string.Empty,
            entity.Material?.Description ?? string.Empty,
            entity.ReceivedQty,
            entity.Material?.Unit ?? string.Empty
        );
    }

    // -------------------- Entity -> ListDto --------------------
    public static ReceivedMaterialListDto ToListDto(this ReceivedMaterial entity)
    {
        return new ReceivedMaterialListDto(
            entity.Id,
            entity.Material?.MaterialNumber ?? string.Empty,
            entity.Material?.Description ?? string.Empty,
            entity.ReceivedQty,
            entity.Material?.Unit ?? string.Empty
        );
    }

    // -------------------- UpsertDto -> Entity (Create) --------------------
    // ✅ ใช้ใน ReceivedProcessMapping (ไม่ต้องส่ง ReceivedProcessId)
    public static ReceivedMaterial ToEntity(this ReceivedMaterialUpsertDto dto)
    {
        return new ReceivedMaterial
        {
            MaterialId = dto.MaterialId,
            ReceivedQty = dto.ReceivedQty
        };
    }

    // ✅ ใช้ใน ReceivedMaterialEndpoints (ต้องการ ReceivedProcessId)
    public static ReceivedMaterial ToEntity(this ReceivedMaterialUpsertDto dto, int receivedProcessId)
    {
        return new ReceivedMaterial
        {
            ReceivedProcessId = receivedProcessId,
            MaterialId = dto.MaterialId,
            ReceivedQty = dto.ReceivedQty
        };
    }

    // -------------------- Update Entity จาก UpsertDto --------------------
    public static void UpdateEntity(this ReceivedMaterial entity, ReceivedMaterialUpsertDto dto)
    {
        entity.MaterialId = dto.MaterialId;
        entity.ReceivedQty = dto.ReceivedQty;
    }
}
