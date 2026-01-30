using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Extensions;

namespace WorkOrderApplication.API.Mappings;

public static class PreparingProcessMapping
{
    // -------------------- Entity → DTO --------------------
    public static PreparingProcessDetailsDto ToDetailsDto(this PreparingProcess entity)
        => new PreparingProcessDetailsDto(
            entity.Id,
            entity.PreparedDate.ToICT(),
            entity.PreparingByUserId,
            entity.PreparingBy?.UserName ?? string.Empty,
            entity.OrderProcessId,

            entity.PreparingMaterials.Select(pm => pm.ToDetailsDto()).ToList()
        );

    public static PreparingProcessListDto ToListDto(this PreparingProcess entity)
        => new PreparingProcessListDto(
            entity.Id,
            entity.PreparedDate.ToICT(),
            entity.OrderProcessId,
            entity.PreparingBy?.UserName ?? string.Empty,

            entity.PreparingMaterials.Count
        );

    // -------------------- DTO → Entity --------------------
    public static PreparingProcess ToEntity(this PreparingProcessUpsertDto dto)
        => new PreparingProcess
        {
            PreparingByUserId = dto.PreparingByUserId,
            OrderProcessId = dto.OrderProcessId,

            PreparedDate = DateTime.UtcNow,
            PreparingMaterials = dto.PreparingMaterials.Select(pm => pm.ToEntity()).ToList()
        };

    public static void UpdateEntity(this PreparingProcess entity, PreparingProcessUpsertDto dto)
    {
        entity.PreparingByUserId = dto.PreparingByUserId;
        entity.OrderProcessId = dto.OrderProcessId;
        entity.PreparedDate = DateTime.UtcNow;
        entity.PreparingMaterials = dto.PreparingMaterials.Select(pm => pm.ToEntity()).ToList();
    }
}
