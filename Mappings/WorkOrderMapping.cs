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
            entity.Order,
            entity.OrderType,
            entity.Plant,
            entity.Material,
            entity.Quantity,
            entity.Unit,
            entity.BasicFinishDate,
            entity.DefaultLine,
            entity.CreatedDate.ToICT()
        );

    // -------------------- Entity → DetailsDto --------------------
    public static WorkOrderDetailsDto ToDetailsDto(this WorkOrder entity)
        => new WorkOrderDetailsDto(
            entity.Id,
            entity.Order,
            entity.OrderType,
            entity.Plant,
            entity.Material,
            entity.Quantity,
            entity.Unit,
            entity.BasicFinishDate,
            entity.DefaultLine,
            entity.CreatedDate.ToICT(),
            entity.UpdatedDate?.ToICT(),
            entity.Materials.Select(m => m.ToDetailsDto()).ToList()
        );

    // -------------------- CreateDto → Entity --------------------
    public static WorkOrder ToEntity(this WorkOrderCreateDto dto)
        => new WorkOrder
        {
            Order = dto.Order,
            OrderType = dto.OrderType,
            Plant = dto.Plant,
            Material = dto.Material,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            BasicFinishDate = dto.BasicFinishDate,
            CreatedDate = DateTime.UtcNow,
            Materials = dto.Materials.Select(m => m.ToEntity()).ToList()
        };

    // -------------------- Update Entity จาก UpdateDto --------------------
    public static void UpdateFromDto(this WorkOrder entity, WorkOrderUpdateDto dto)
    {
        entity.Order = dto.Order;
        entity.OrderType = dto.OrderType;
        entity.Plant = dto.Plant;
        entity.Material = dto.Material;
        entity.Quantity = dto.Quantity;
        entity.Unit = dto.Unit;
        entity.BasicFinishDate = dto.BasicFinishDate;
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
