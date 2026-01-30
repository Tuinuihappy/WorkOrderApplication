using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Extensions;

namespace WorkOrderApplication.API.Mappings;

public static class CancelledProcessMapping
{
    // -------------------- Entity -> DetailsDto --------------------
    public static CancelledProcessDetailsDto ToDetailsDto(this CancelledProcess entity)
    {
        return new CancelledProcessDetailsDto(
            entity.Id,
            entity.CancelledDate.ToICT(),
            entity.Reason,
            entity.CancelledByUserId,
            entity.CancelledBy?.UserName,
            entity.OrderProcessId
        );
    }

    // -------------------- Entity -> ListDto --------------------
    public static CancelledProcessListDto ToListDto(this CancelledProcess entity)
    {
        return new CancelledProcessListDto(
            entity.Id,
            entity.CancelledDate.ToICT(),
            entity.Reason,
            entity.CancelledBy?.UserName,
            entity.OrderProcessId
        );
    }

    // -------------------- UpsertDto -> Entity (Create) --------------------
    // ✅ กำหนดเวลา CancelledDate อัตโนมัติที่นี่
    public static CancelledProcess ToEntity(this CancelledProcessUpsertDto dto)
    {
        return new CancelledProcess
        {
            CancelledDate = DateTime.UtcNow, // ระบบเซ็ตเวลาเอง
            Reason = dto.Reason,
            CancelledByUserId = dto.CancelledByUserId,
            OrderProcessId = dto.OrderProcessId
        };
    }

    // -------------------- Update Entity จาก UpsertDto --------------------
    // ✅ ไม่อัปเดต CancelledDate — เพราะไม่ควรแก้ timestamp ของการยกเลิก
    public static void UpdateEntity(this CancelledProcess entity, CancelledProcessUpsertDto dto)
    {
        entity.Reason = dto.Reason;
        entity.CancelledByUserId = dto.CancelledByUserId;
        entity.OrderProcessId = dto.OrderProcessId;
        // ❌ ไม่แตะต้อง CancelledDate
    }
}
