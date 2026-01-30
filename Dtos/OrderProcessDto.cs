namespace WorkOrderApplication.API.Dtos;

// -------------------- รายละเอียดเต็ม --------------------
public record OrderProcessDetailsDto(
    int Id,
    string OrderNumber,
    DateTime CreatedDate,
    DateTime? TimeToUse, // ✅ เพิ่ม TimeToUse
    string Status,
    int CreatedByUserId,
    string CreatedByName,
    int WorkOrderId,
    WorkOrderDetailsDto WorkOrder,
    List<OrderMaterialDetailsDto> OrderMaterials,
    ConfirmProcessDetailsDto? ConfirmProcess,
    PreparingProcessDetailsDto? PreparingProcess,
    ShipmentProcessDto? ShipmentProcess,
    ReceivedProcessDetailsDto? Receive,
    CancelledProcessDetailsDto? CancelledProcess,
    ReturnProcessDetailsDto? ReturnProcess
);

// -------------------- ใช้สำหรับ Insert / Update --------------------
public record OrderProcessUpsertDto(
    string OrderNumber,
    int WorkOrderId,
    int CreatedByUserId,
    DateTimeOffset? TimeToUse, // ✅ Client ใส่แค่เวลา เช่น "14:30:00"
    List<OrderMaterialUpsertDto> OrderMaterials
);

// -------------------- สำหรับ List / Table View --------------------
public record OrderProcessListDto(
    int Id,
    string OrderNumber,
    DateTime CreatedDate,
    DateTime? TimeToUse, // ✅ แสดงเวลาที่จะใช้
    string Status,
    string WorkOrderNumber,
    string CreatedByName,
    string? LineName,
    string? SourceStation,
    string? DestinationStation,
    string? ExecuteVehicleName
);
