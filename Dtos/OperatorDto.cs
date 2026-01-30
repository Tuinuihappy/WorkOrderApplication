namespace WorkOrderApplication.API.Dtos;

// -------------------- รายละเอียดเต็ม --------------------
public record UserDetailsDto(
    int Id,
    string UserName,
    string EmployeeId,
    string Position,
    string Department,
    string Shift,
    string? ContactNumber,
    string Email,
    DateTime CreatedDate,
    DateTime UpdatedDate
);

// -------------------- ใช้สำหรับ Insert/Update --------------------
public record UserUpsertDto(
    string UserName,
    string EmployeeId,
    string Position,
    string Department,
    string Shift,
    string? ContactNumber,
    string Email
);

// -------------------- สำหรับ List / Table View --------------------
public record UserListDto(
    int Id,
    string UserName,
    string EmployeeId,
    string Position,
    string Department,
    string Shift
);
