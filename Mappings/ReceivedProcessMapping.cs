using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Extensions;
using WorkOrderApplication.API.Mappings; // ✅ ต้องมีเพื่อใช้ ReceivedMaterialMapping

namespace WorkOrderApplication.API.Mappings;

public static class ReceivedProcessMapping
{
    // -------------------- Entity -> DetailsDto --------------------
    public static ReceivedProcessDetailsDto ToDetailsDto(this ReceivedProcess entity)
    {
        return new ReceivedProcessDetailsDto(
            entity.Id,
            entity.ReceivedDate.ToICT(),
            entity.ReceivedByUserId,
            entity.ReceivedBy?.UserName ?? string.Empty,
            entity.OrderProcessId,
            entity.ShortageReason,
            entity.ReceivedMaterials.Select(rm => rm.ToDetailsDto()).ToList()
        );
    }

    // -------------------- Entity -> ListDto --------------------
    public static ReceivedProcessListDto ToListDto(this ReceivedProcess entity)
    {
        return new ReceivedProcessListDto(
            entity.Id,
            entity.ReceivedDate.ToICT(),
            entity.ReceivedBy?.UserName ?? string.Empty,
            entity.OrderProcessId
        );
    }

    // -------------------- UpsertDto -> Entity (Create) --------------------
    public static ReceivedProcess ToEntity(this ReceivedProcessUpsertDto dto)
    {
        return new ReceivedProcess
        {
            ReceivedDate = DateTime.UtcNow,
            ReceivedByUserId = dto.ReceivedByUserId,
            OrderProcessId = dto.OrderProcessId,
            ShortageReason = dto.ShortageReason,
            ReceivedMaterials = dto.ReceivedMaterials.Select(rm => rm.ToEntity()).ToList()
        };
    }

    // -------------------- Update Entity จาก UpsertDto --------------------
    public static void UpdateEntity(this ReceivedProcess entity, ReceivedProcessUpsertDto dto)
    {
        entity.ReceivedByUserId = dto.ReceivedByUserId;
        entity.OrderProcessId = dto.OrderProcessId;
        entity.ReceivedDate = DateTime.UtcNow;
        entity.ReceivedMaterials = dto.ReceivedMaterials.Select(rm => rm.ToEntity()).ToList();
    }
}
