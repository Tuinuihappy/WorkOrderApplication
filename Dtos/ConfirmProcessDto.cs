namespace WorkOrderApplication.API.Dtos;

// -------------------- รายละเอียดเต็ม --------------------
public record ConfirmProcessDetailsDto(
    int Id,
    int OrderProcessId,
    DateTime ConfirmedDate
);

// -------------------- ใช้สำหรับ Insert --------------------
public record ConfirmProcessUpsertDto(
    int OrderProcessId
);

// -------------------- สำหรับ List / Table View --------------------
public record ConfirmProcessListDto(
    int Id,
    int OrderProcessId,
    DateTime ConfirmedDate
);
