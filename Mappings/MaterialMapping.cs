using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Mappings;

public static class MaterialMapping
{
    // -------------------- Entity → ListDto --------------------
    public static MaterialListDto ToListDto(this Material entity)
        => new MaterialListDto(
            entity.Id,
            entity.MaterialNumber,
            entity.Description,
            entity.Quantity,
            entity.Unit
        );

    // -------------------- Entity → DetailsDto --------------------
    public static MaterialDetailsDto ToDetailsDto(this Material entity)
        => new MaterialDetailsDto(
            entity.Id,
            entity.MaterialNumber,
            entity.Description,
            entity.Quantity,
            entity.RequestPerHour,
            entity.Unit,
            entity.WorkOrderId,
            entity.WorkOrder?.WorkOrderNumber ?? string.Empty
        );

    // -------------------- CreateDto → Entity --------------------
    public static Material ToEntity(this MaterialCreateDto dto)
        => new Material
        {
            MaterialNumber = dto.MaterialNumber,
            Description = dto.Description,
            Quantity = dto.Quantity,
            RequestPerHour = dto.RequestPerHour,
            Unit = dto.Unit
        };

    // -------------------- UpdateDto → Entity (ใช้ตอน reset Materials ใหม่) --------------------
    public static Material ToEntity(this MaterialUpdateDto dto)
        => new Material
        {
            // ❌ อย่า copy Id เดิม ให้ EF Core generate ใหม่
            MaterialNumber = dto.MaterialNumber,
            Description = dto.Description,
            Quantity = dto.Quantity,
            RequestPerHour = dto.RequestPerHour,
            Unit = dto.Unit
        };

    public static Material ToEntity(this MaterialCreateDto dto, int workOrderId)
        => new Material
        {
            MaterialNumber = dto.MaterialNumber,
            Description = dto.Description,
            Quantity = dto.Quantity,
            RequestPerHour = dto.RequestPerHour,
            Unit = dto.Unit,
            WorkOrderId = workOrderId
    };

    // -------------------- UpdateDto → UpdateEntity (แก้ไขของเดิม) --------------------
    public static void UpdateEntity(this Material entity, MaterialUpdateDto dto)
    {
        entity.MaterialNumber = dto.MaterialNumber;
        entity.Description = dto.Description;
        entity.Quantity = dto.Quantity;
        entity.RequestPerHour = dto.RequestPerHour;
        entity.Unit = dto.Unit;
    }
}
