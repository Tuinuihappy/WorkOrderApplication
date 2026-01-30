using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Extensions;

namespace WorkOrderApplication.API.Mappings;

public static class ReturnProcessMapping
{
    // -------------------- Entity -> DetailsDto --------------------
    public static ReturnProcessDetailsDto ToDetailsDto(this ReturnProcess entity)
        => new(
            entity.Id,
            entity.ReturnDate.ToICT(),
            entity.Reason,
            entity.ReturnByUserId,
            entity.ReturnByUser?.UserName ?? string.Empty,
            entity.OrderProcessId
        );

    // -------------------- Entity -> ListDto --------------------
    public static ReturnProcessListDto ToListDto(this ReturnProcess entity)
        => new(
            entity.Id,
            entity.ReturnDate.ToICT(),
            entity.Reason,
            entity.ReturnByUser?.UserName ?? string.Empty,
            entity.OrderProcessId
        );

    // -------------------- UpsertDto -> Entity (Create) --------------------
    public static ReturnProcess ToEntity(this ReturnProcessUpsertDto dto)
        => new()
        {
            ReturnDate = DateTime.UtcNow, // ✅ กำหนดเวลาบันทึกข้อมูลจริง
            Reason = dto.Reason,
            ReturnByUserId = dto.ReturnByUserId,
            OrderProcessId = dto.OrderProcessId
        };

    // -------------------- Update Entity จาก UpsertDto --------------------
    public static void UpdateEntity(this ReturnProcess entity, ReturnProcessUpsertDto dto)
    {
        entity.Reason = dto.Reason;
        entity.ReturnByUserId = dto.ReturnByUserId;
        entity.OrderProcessId = dto.OrderProcessId;
    }
}
