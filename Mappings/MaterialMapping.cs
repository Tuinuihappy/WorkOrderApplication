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
            entity.MaterialDescription,
            entity.ReqmntQty,
            entity.QtyWthdrn,
            entity.BUn,
            entity.OpAc,
            entity.SortString,
            entity.SLoc
        );

    // -------------------- Entity → DetailsDto --------------------
    public static MaterialDetailsDto ToDetailsDto(this Material entity)
        => new MaterialDetailsDto(
            entity.Id,
            entity.MaterialNumber,
            entity.MaterialDescription,
            entity.ReqmntQty,
            entity.QtyWthdrn,
            entity.BUn,
            entity.OpAc,
            entity.SortString,
            entity.SLoc,
            entity.WorkOrderId,
            entity.WorkOrder?.Order ?? string.Empty
        );

    // -------------------- CreateDto → Entity --------------------
    public static Material ToEntity(this MaterialCreateDto dto)
        => new Material
        {
            MaterialNumber = dto.MaterialNumber,
            MaterialDescription = dto.MaterialDescription,
            ReqmntQty = dto.ReqmntQty,
            QtyWthdrn = dto.QtyWthdrn,
            BUn = dto.BUn,
            OpAc = dto.OpAc,
            SortString = dto.SortString,
            SLoc = dto.SLoc
        };

    // -------------------- UpdateDto → Entity (ใช้ตอน reset Materials ใหม่) --------------------
    public static Material ToEntity(this MaterialUpdateDto dto)
        => new Material
        {
            // ❌ อย่า copy Id เดิม ให้ EF Core generate ใหม่
            MaterialNumber = dto.MaterialNumber,
            MaterialDescription = dto.MaterialDescription,
            ReqmntQty = dto.ReqmntQty,
            QtyWthdrn = dto.QtyWthdrn,
            BUn = dto.BUn,
            OpAc = dto.OpAc,
            SortString = dto.SortString,
            SLoc = dto.SLoc
        };

    public static Material ToEntity(this MaterialCreateDto dto, int workOrderId)
        => new Material
        {
            MaterialNumber = dto.MaterialNumber,
            MaterialDescription = dto.MaterialDescription,
            ReqmntQty = dto.ReqmntQty,
            QtyWthdrn = dto.QtyWthdrn,
            BUn = dto.BUn,
            OpAc = dto.OpAc,
            SortString = dto.SortString,
            SLoc = dto.SLoc,
            WorkOrderId = workOrderId
        };

    // -------------------- UpdateDto → UpdateEntity (แก้ไขของเดิม) --------------------
    public static void UpdateEntity(this Material entity, MaterialUpdateDto dto)
    {
        entity.MaterialNumber = dto.MaterialNumber;
        entity.MaterialDescription = dto.MaterialDescription;
        entity.ReqmntQty = dto.ReqmntQty;
        entity.QtyWthdrn = dto.QtyWthdrn;
        entity.BUn = dto.BUn;
        entity.OpAc = dto.OpAc;
        entity.SortString = dto.SortString;
        entity.SLoc = dto.SLoc;
    }
}
