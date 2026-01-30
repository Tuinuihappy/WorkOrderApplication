namespace WorkOrderApplication.API.Dtos;

// -------------------- สำหรับ Create --------------------
public record WorkOrderCreateDto(
    string WorkOrderNumber,
    string LineName,
    string ModelName,
    int Quantity,
    int CreatedByUserId,
    List<MaterialCreateDto> Materials     // ✅ สร้างพร้อมกับ Materials
);

// -------------------- สำหรับ Update --------------------
public record WorkOrderUpdateDto(
    string WorkOrderNumber,
    string LineName,
    string ModelName,
    int Quantity,
    int? UpdatedByUserId,
    List<MaterialUpdateDto> Materials     // ✅ อัพเดทพร้อม Materials
);

// -------------------- รายละเอียดเต็ม --------------------
public record WorkOrderDetailsDto(
    int Id,
    string WorkOrderNumber,
    string LineName,
    string ModelName,
    int Quantity,
    DateTime CreatedDate,
    DateTime? UpdatedDate,
    int CreatedByUserId,
    string CreatedByName,
    int? UpdatedByUserId,
    string? UpdatedByName,
    List<MaterialDetailsDto> Materials          // ✅ แสดง Materials แบบย่อ
);

// -------------------- สำหรับแสดงในตาราง --------------------
public record WorkOrderListDto(
    int Id,
    string WorkOrderNumber,
    string LineName,
    string ModelName,
    int Quantity,
    string CreatedByName,
    DateTime CreatedDate
);
