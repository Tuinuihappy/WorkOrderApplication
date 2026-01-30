namespace WorkOrderApplication.API.Dtos;

// -------------------- รายละเอียดเต็ม --------------------
public record CancelledProcessDetailsDto(
    int Id,
    DateTime CancelledDate,
    string Reason,
    int? CancelledByUserId,
    string? CancelledByUserName,
    int OrderProcessId
);

// -------------------- ใช้สำหรับ Insert / Update --------------------
public record CancelledProcessUpsertDto(
    string Reason,
    int? CancelledByUserId,
    int OrderProcessId
);

// -------------------- สำหรับ List / Table View --------------------
public record CancelledProcessListDto(
    int Id,
    DateTime CancelledDate,
    string Reason,
    string? CancelledByUserName,
    int OrderProcessId
);
