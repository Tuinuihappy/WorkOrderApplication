namespace WorkOrderApplication.API.Dtos;

public record OrderRecordResponseDto(
    string Code,
    string Message,
    string MsgDetail,
    OrderRecordResultDto Result,
    string Tid
);

public record OrderRecordDto(
    string AppointExecuteTime,
    int AppointMapId,
    int AppointStationId,
    int AppointVehicleGroupId,
    string AppointVehicleGroupName,
    string AppointVehicleKey,
    int ArriveTime,
    string ArriveTimeMsg,
    int ChangeVehicle,
    string ChangeVehicleReason,
    string ChangeVehicleRecord,
    DateTime CreateTime,
    string CreatedBy,
    double Distance,
    string EndStationName,
    int EndStationNo,
    int Eta,
    string ExecuteTime,
    string ExecuteVehicleKey,
    string ExecuteVehicleName,
    int ExecutingIndex,
    string FailReason,
    int Id,
    int IsAppointEnable,
    int IsDeleted,
    int LockStatus,
    string ModifiedBy,
    string OrderId,
    string OrderName,
    int OrderState,
    int OrderType,
    int Priority,
    int PriorityQueue,
    double Progress,
    string Source,
    string StartEndStationName,
    string StartEndStationNameDetail,
    string StartStationName,
    int StartStationNo,
    string TaskState,
    double TotalCosts,
    DateTime UpdateTime,
    string UpperId,
    int UserId
);

public record OrderRecordResultDto(
    int Current,
    bool OptimizeCountSql,
    List<OrderRecordDto> Records,
    int Pages,
    bool SearchCount,
    int Size,
    int Total
);