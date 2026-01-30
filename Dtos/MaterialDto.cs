namespace WorkOrderApplication.API.Dtos;

// -------------------- สำหรับ Create --------------------
public record MaterialCreateDto(
    string MaterialNumber,
    string Description,
    int Quantity,
    int RequestPerHour,
    string Unit
);

// -------------------- สำหรับ Update --------------------
public record MaterialUpdateDto(
    int? Id,
    string MaterialNumber,
    string Description,
    int Quantity,
    int RequestPerHour,
    string Unit
);

// -------------------- สำหรับ List / Table View --------------------
public record MaterialListDto(
    int Id,
    string MaterialNumber,
    string Description,
    int Quantity,
    string Unit
);

// -------------------- รายละเอียดเต็ม --------------------
public record MaterialDetailsDto(
    int Id,
    string MaterialNumber,
    string Description,
    int Quantity,
    int RequestPerHour,
    string Unit,
    int WorkOrderId,
    string WorkOrderNumber
);
