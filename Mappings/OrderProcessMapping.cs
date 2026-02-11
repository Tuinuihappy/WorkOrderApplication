using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Extensions;

namespace WorkOrderApplication.API.Mappings;

public static class OrderProcessMapping
{
    // -------------------- Entity → DetailsDto --------------------
    public static OrderProcessDetailsDto ToDetailsDto(this OrderProcess entity)
        => new OrderProcessDetailsDto(
            entity.Id,
            entity.OrderNumber,
            entity.CreatedDate.ToICT(),
            entity.TimeToUse?.ToICT(),   // ✅ แปลงกลับ ICT เวลาอ่านออก
            entity.Status,
            entity.CreatedByUserId,
            entity.CreatedBy?.UserName ?? string.Empty,
            entity.WorkOrderId,
            entity.WorkOrder?.ToDetailsDto(), // ✅ Add null check
            entity.OrderMaterials?.Select(om => om.ToDetailsDto()).ToList() ?? new List<OrderMaterialDetailsDto>(), // ✅ Add null check
            entity.ConfirmProcess?.ToDetailsDto(),
            entity.PreparingProcess?.ToDetailsDto(),
            entity.ShipmentProcess?.ToDto(),
            entity.ReceiveProcess?.ToDetailsDto(),
            entity.CancelledProcess?.ToDetailsDto(),
            entity.ReturnProcess?.ToDetailsDto(),
            entity.PreparingProcess?.ShortageReason // ✅ Map ShortageReason
        );

    // -------------------- Entity → ListDto --------------------
    public static OrderProcessListDto ToListDto(this OrderProcess entity)
        => new OrderProcessListDto(
            entity.Id,
            entity.OrderNumber,
            entity.CreatedDate.ToICT(),
            entity.TimeToUse?.ToICT(),
            entity.Status,
            entity.WorkOrder?.Order ?? string.Empty,
            entity.CreatedBy?.UserName ?? string.Empty,
            entity.WorkOrder?.OrderType,                          // ✅ เพิ่ม OrderType
            entity.ShipmentProcess?.SourceStation,               // ✅ เพิ่ม sourceStation
            entity.ShipmentProcess?.DestinationStation,          // ✅ เพิ่ม destinationStation
            entity.ShipmentProcess?.ExecuteVehicleName           // ✅ เพิ่ม executeVehicleName
        );

    // -------------------- Dto (Upsert) → Entity --------------------
    public static OrderProcess ToEntity(this OrderProcessUpsertDto dto)
    {
        return new OrderProcess
        {
            OrderNumber = dto.OrderNumber,
            WorkOrderId = dto.WorkOrderId,
            CreatedByUserId = dto.CreatedByUserId,
            Status = "Order Placed",
            CreatedDate = DateTime.UtcNow,
            TimeToUse = dto.TimeToUse?.UtcDateTime,   // ✅ แปลงเป็น UTC ก่อนเก็บ
            OrderMaterials = dto.OrderMaterials?
                .Select(m => m.ToEntity())
                .ToList() ?? new List<OrderMaterial>()
        };
    }

    // -------------------- Update Entity from Dto --------------------
    public static void UpdateEntity(this OrderProcess entity, OrderProcessUpsertDto dto)
    {
        entity.OrderNumber = dto.OrderNumber;
        entity.WorkOrderId = dto.WorkOrderId;
        entity.CreatedByUserId = dto.CreatedByUserId;
        entity.TimeToUse = dto.TimeToUse?.UtcDateTime;

        // ✅ อัปเดต Materials (replace ทั้งชุด)
        entity.OrderMaterials.Clear();
        if (dto.OrderMaterials != null)
        {
            foreach (var m in dto.OrderMaterials)
                entity.OrderMaterials.Add(m.ToEntity());
        }
    }

}
