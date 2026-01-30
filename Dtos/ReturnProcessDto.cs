namespace WorkOrderApplication.API.Dtos;

// -------------------- รายละเอียดเต็ม --------------------
public record ReturnProcessDetailsDto(
    int Id,
    DateTime ReturnDate,
    string Reason,
    int ReturnByUserId,
    string ReturnByUserName,
    int OrderProcessId
);

// -------------------- ใช้สำหรับ Insert / Update --------------------
public record ReturnProcessUpsertDto(
    string Reason,
    int ReturnByUserId,
    int OrderProcessId
);

// -------------------- สำหรับ List / Table View --------------------
public record ReturnProcessListDto(
    int Id,
    DateTime ReturnDate,
    string Reason,
    string ReturnByUserName,
    int OrderProcessId
);
