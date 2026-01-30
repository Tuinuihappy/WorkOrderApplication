using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Extensions;

namespace WorkOrderApplication.API.Mappings;

public static class WorkOrderMapping
{
    // -------------------- Entity → ListDto --------------------
    public static WorkOrderListDto ToListDto(this WorkOrder entity)
        => new WorkOrderListDto(
            entity.Id,
            entity.WorkOrderNumber,
            entity.LineName,
            entity.ModelName,
            entity.Quantity,
            entity.CreatedBy?.UserName ?? string.Empty,
            entity.CreatedDate.ToICT()
        );

    // -------------------- Entity → DetailsDto --------------------
    public static WorkOrderDetailsDto ToDetailsDto(this WorkOrder entity)
        => new WorkOrderDetailsDto(
            entity.Id,
            entity.WorkOrderNumber,
            entity.LineName,
            entity.ModelName,
            entity.Quantity,
            entity.CreatedDate.ToICT(),
            entity.UpdatedDate?.ToICT(),
            entity.CreatedByUserId,
            entity.CreatedBy?.UserName ?? string.Empty,
            entity.UpdatedByUserId,
            entity.UpdatedBy?.UserName,
            entity.Materials.Select(m => m.ToDetailsDto()).ToList()
        );

    // -------------------- CreateDto → Entity --------------------
    public static WorkOrder ToEntity(this WorkOrderCreateDto dto)
        => new WorkOrder
        {
            WorkOrderNumber = dto.WorkOrderNumber,
            LineName = dto.LineName,
            ModelName = dto.ModelName,
            Quantity = dto.Quantity,
            CreatedByUserId = dto.CreatedByUserId,
            CreatedDate = DateTime.UtcNow,
            Materials = dto.Materials.Select(m => m.ToEntity()).ToList()
        };

    // -------------------- Update Entity จาก UpdateDto --------------------
    public static void UpdateFromDto(this WorkOrder entity, WorkOrderUpdateDto dto)
    {
        entity.WorkOrderNumber = dto.WorkOrderNumber;
        entity.LineName = dto.LineName;
        entity.ModelName = dto.ModelName;
        entity.Quantity = dto.Quantity;
        entity.UpdatedByUserId = dto.UpdatedByUserId;
        entity.UpdatedDate = DateTime.UtcNow;

        // ❌ ไม่สนใจ Material.Id เดิม
        // ลบทั้งหมดแล้วสร้างใหม่
        entity.Materials.Clear();
        foreach (var m in dto.Materials)
        {
            var newMaterial = m.ToEntity(); // ให้ DB generate Id ใหม่เสมอ
            entity.Materials.Add(newMaterial);
        }
    }
}
