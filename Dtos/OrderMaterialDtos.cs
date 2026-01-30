namespace WorkOrderApplication.API.Dtos;

// -------------------- รายละเอียดเต็ม --------------------
public record OrderMaterialDetailsDto(
    int Id,
    int OrderProcessId,
    int MaterialId,
    string MaterialNumber,
    string Description,
    int OrderQty,
    string Unit
);
// -------------------- ใช้สำหรับ Insert / Update --------------------
public record OrderMaterialUpsertDto(
    int MaterialId,
    int OrderQty
);
// -------------------- สำหรับ List / Table View --------------------
public record OrderMaterialListDto(
    int Id,
    int MaterialId,
    string MaterialNumber,
    int OrderQty
);
