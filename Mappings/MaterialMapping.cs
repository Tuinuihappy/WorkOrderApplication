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
            entity.WithdrawnQuantity,
            entity.Unit,
            entity.OperationActivity,
            entity.SortString,
            entity.StorageLocation
        );

    // -------------------- Entity → DetailsDto --------------------
    public static MaterialDetailsDto ToDetailsDto(this Material entity)
        => new MaterialDetailsDto(
            entity.Id,
            entity.MaterialNumber,
            entity.Description,
            entity.Quantity,
            entity.WithdrawnQuantity,
            entity.Unit,
            entity.OperationActivity,
            entity.SortString,
            entity.StorageLocation,
            entity.WorkOrderId,
            entity.WorkOrder?.Order ?? string.Empty
        );

    // -------------------- CreateDto → Entity --------------------
    public static Material ToEntity(this MaterialCreateDto dto)
        => new Material
        {
            MaterialNumber = dto.MaterialNumber,
            Description = dto.Description,
            Quantity = dto.Quantity,
            WithdrawnQuantity = dto.WithdrawnQuantity,
            Unit = dto.Unit,
            OperationActivity = dto.OperationActivity,
            SortString = dto.SortString,
            StorageLocation = dto.StorageLocation
        };

    // -------------------- UpdateDto → Entity (ใช้ตอน reset Materials ใหม่) --------------------
    public static Material ToEntity(this MaterialUpdateDto dto)
        => new Material
        {
            // ❌ อย่า copy Id เดิม ให้ EF Core generate ใหม่
            MaterialNumber = dto.MaterialNumber,
            Description = dto.Description,
            Quantity = dto.Quantity,
            WithdrawnQuantity = dto.WithdrawnQuantity,
            Unit = dto.Unit,
            OperationActivity = dto.OperationActivity,
            SortString = dto.SortString,
            StorageLocation = dto.StorageLocation
        };

    public static Material ToEntity(this MaterialCreateDto dto, int workOrderId)
        => new Material
        {
            MaterialNumber = dto.MaterialNumber,
            Description = dto.Description,
            Quantity = dto.Quantity,
            WithdrawnQuantity = dto.WithdrawnQuantity,
            Unit = dto.Unit,
            OperationActivity = dto.OperationActivity,
            SortString = dto.SortString,
            StorageLocation = dto.StorageLocation,
            RequestPerHour = dto.RequestPerHour,
            WorkOrderId = workOrderId
        };

    // -------------------- UpdateDto → UpdateEntity (แก้ไขของเดิม) --------------------
    public static void UpdateEntity(this Material entity, MaterialUpdateDto dto)
    {
        entity.MaterialNumber = dto.MaterialNumber;
        entity.Description = dto.Description;
        entity.Quantity = dto.Quantity;
        entity.WithdrawnQuantity = dto.WithdrawnQuantity;
        entity.Unit = dto.Unit;
        entity.OperationActivity = dto.OperationActivity;
        entity.SortString = dto.SortString;
        entity.StorageLocation = dto.StorageLocation;
    }
}
