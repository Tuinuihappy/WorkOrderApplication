namespace WorkOrderApplication.API.Dtos;

// -------------------- รายละเอียดเต็ม --------------------
public record UserDetailsDto(
    int Id,
    string UserName,
    string EmployeeId,
    string Position,
    string Shift,
    string Role,
    DateTime CreatedDate,
    DateTime UpdatedDate
);

// -------------------- ใช้สำหรับ Insert/Update --------------------
public record UserUpsertDto(
    string UserName,
    string EmployeeId,
    string Position,
    string Shift,
    string? Password, // Optional string สำหรับตอน Update อาจจะไม่เปลี่ยนรหัสผ่าน
    string Role = "User"
);

// -------------------- สำหรับ List / Table View --------------------
public record UserListDto(
    int Id,
    string UserName,
    string EmployeeId,
    string Position,
    string Shift,
    string Role
);
