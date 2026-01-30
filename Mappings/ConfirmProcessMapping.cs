using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Extensions;

namespace WorkOrderApplication.API.Mappings;

public static class ConfirmProcessMapping
{
    // -------------------- Entity → DetailsDto --------------------
    public static ConfirmProcessDetailsDto ToDetailsDto(this ConfirmProcess entity)
        => new ConfirmProcessDetailsDto(
            entity.Id,
            entity.OrderProcessId,
            entity.ConfirmedDate.ToICT() // ✅ แปลงเป็นเวลา ICT ก่อนส่งออก
        );

    // -------------------- Entity → ListDto --------------------
    public static ConfirmProcessListDto ToListDto(this ConfirmProcess entity)
        => new ConfirmProcessListDto(
            entity.Id,
            entity.OrderProcessId,
            entity.ConfirmedDate.ToICT() // ✅ เช่นเดียวกัน
        );

    // -------------------- UpsertDto → Entity --------------------
    public static ConfirmProcess ToEntity(this ConfirmProcessUpsertDto dto)
        => new ConfirmProcess
        {
            OrderProcessId = dto.OrderProcessId,
            ConfirmedDate = DateTime.UtcNow // ✅ เก็บเป็น UTC ใน DB
        };

    // -------------------- Update Entity --------------------
    public static void UpdateEntity(this ConfirmProcess entity, ConfirmProcessUpsertDto dto)
    {
        entity.OrderProcessId = dto.OrderProcessId;
        entity.ConfirmedDate = DateTime.UtcNow; // ✅ update เป็น UTC
    }
}
