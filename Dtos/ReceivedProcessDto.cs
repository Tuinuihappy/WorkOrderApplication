namespace WorkOrderApplication.API.Dtos;

// -------------------- รายละเอียดเต็ม --------------------
public record ReceivedProcessDetailsDto(
    int Id,
    DateTime ReceivedDate,
    int ReceivedByUserId,
    string ReceivedByName,
    int OrderProcessId,
    string? ShortageReason,
    List<ReceivedMaterialDetailsDto> ReceivedMaterials
);

// -------------------- ใช้สำหรับ Insert / Update --------------------
public record ReceivedProcessUpsertDto(
    int ReceivedByUserId,
    int OrderProcessId,
    string? ShortageReason,
    List<ReceivedMaterialUpsertDto> ReceivedMaterials
);

// -------------------- สำหรับ List / Table View --------------------
public record ReceivedProcessListDto(
    int Id,
    DateTime ReceivedDate,
    string ReceivedByName,
    int OrderProcessId
);


