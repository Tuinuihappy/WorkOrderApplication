namespace WorkOrderApplication.API.Dtos;

// -------------------- สำหรับ Create --------------------
public record MaterialCreateDto(
    string MaterialNumber,
    string Description,
    decimal Quantity,
    decimal WithdrawnQuantity,
    string Unit,
    string? OperationActivity,
    string? SortString,
    string? StorageLocation
);

// -------------------- สำหรับ Update --------------------
public record MaterialUpdateDto(
    int? Id,
    string MaterialNumber,
    string Description,
    decimal Quantity,
    decimal WithdrawnQuantity,
    string Unit,
    string? OperationActivity,
    string? SortString,
    string? StorageLocation
);

// -------------------- สำหรับ List / Table View --------------------
public record MaterialListDto(
    int Id,
    string MaterialNumber,
    string Description,
    decimal Quantity,
    decimal WithdrawnQuantity,
    string Unit,
    string? OperationActivity,
    string? SortString,
    string? StorageLocation
);

// -------------------- รายละเอียดเต็ม --------------------
public record MaterialDetailsDto(
    int Id,
    string MaterialNumber,
    string Description,
    decimal Quantity,
    decimal WithdrawnQuantity,
    string Unit,
    string? OperationActivity,
    string? SortString,
    string? StorageLocation,
    int WorkOrderId,
    string Order
);
