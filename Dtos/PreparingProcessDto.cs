namespace WorkOrderApplication.API.Dtos;

// -------------------- รายละเอียดเต็ม --------------------
public record PreparingProcessDetailsDto(
    int Id,
    DateTime PreparedDate,
    int PreparingByUserId,
    string PreparingByName,
    int OrderProcessId,
    List<PreparingMaterialDetailsDto> PreparingMaterials
);

// -------------------- ใช้สำหรับ Insert / Update --------------------
public record PreparingProcessUpsertDto(
    int PreparingByUserId,
    int OrderProcessId,
    List<PreparingMaterialUpsertDto> PreparingMaterials
);

// -------------------- สำหรับ List / Table View --------------------
public record PreparingProcessListDto(
    int Id,
    DateTime PreparedDate,
    int OrderProcessId,
    string PreparingByName,
    int MaterialsCount
);
