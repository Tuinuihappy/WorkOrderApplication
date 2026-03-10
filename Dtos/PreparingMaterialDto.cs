namespace WorkOrderApplication.API.Dtos;

// -------------------- รายละเอียดเต็ม --------------------
public record PreparingMaterialDetailsDto(
    int Id,
    int PreparingProcessId,
    int MaterialId,
    string MaterialNumber,
    string MaterialDescription,
    decimal PreparedQty,
    string BUn
);

// -------------------- ใช้สำหรับ Insert / Update --------------------
public record PreparingMaterialUpsertDto(
    int MaterialId,
    decimal PreparedQty
);

// -------------------- สำหรับ List / Table View --------------------
public record PreparingMaterialListDto(
    int Id,
    int MaterialId,
    string MaterialNumber,
    decimal PreparedQty
);
