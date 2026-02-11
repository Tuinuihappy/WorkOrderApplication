namespace WorkOrderApplication.API.Dtos;

// -------------------- สำหรับ Create --------------------
public record WorkOrderCreateDto(
    string Order,
    string OrderType,
    string Plant,
    string Material,
    int Quantity,
    string Unit,
    DateTime? BasicFinishDate,
    List<MaterialCreateDto> Materials     // ✅ สร้างพร้อมกับ Materials
);

// -------------------- สำหรับ Update --------------------
public record WorkOrderUpdateDto(
    string Order,
    string OrderType,
    string Plant,
    string Material,
    int Quantity,
    string Unit,
    DateTime? BasicFinishDate,
    List<MaterialUpdateDto> Materials     // ✅ อัพเดทพร้อม Materials
);

// -------------------- รายละเอียดเต็ม --------------------
public record WorkOrderDetailsDto(
    int Id,
    string Order,
    string OrderType,
    string Plant,
    string Material,
    int Quantity,
    string Unit,
    DateTime? BasicFinishDate,
    DateTime CreatedDate,
    DateTime? UpdatedDate,
    List<MaterialDetailsDto> Materials          // ✅ แสดง Materials แบบย่อ
);

// -------------------- สำหรับแสดงในตาราง --------------------
public record WorkOrderListDto(
    int Id,
    string Order,
    string OrderType,
    string Plant,
    string Material,
    int Quantity,
    string Unit,
    DateTime? BasicFinishDate,
    DateTime CreatedDate
);
