namespace WorkOrderApplication.API.Dtos;

// -------------------- รายละเอียดเต็ม --------------------
public record ReceivedMaterialDetailsDto(
    int Id,
    int ReceivedProcessId,
    int MaterialId,
    string MaterialNumber,
    string Description,
    int ReceivedQty,
    string Unit
);

// -------------------- ใช้สำหรับ Insert / Update --------------------
public record ReceivedMaterialUpsertDto(
    int MaterialId,
    int ReceivedQty
);

// -------------------- สำหรับ List / Table View --------------------
public record ReceivedMaterialListDto(
    int Id,
    string MaterialNumber,
    string Description,
    int ReceivedQty,
    string Unit
);