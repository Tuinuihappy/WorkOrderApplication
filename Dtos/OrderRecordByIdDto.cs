using System.Text.Json.Serialization;

namespace WorkOrderApplication.API.Dtos;

public record OrderRecordByIdResponse(
    string Code,
    string Message,
    string MsgDetail,
    OrderRecordByIdDto? Result
);

public record OrderRecordByIdDto(
    int Id,
    string OrderId,
    string OrderName,
    int OrderState,
    int OrderType,
    double Progress,
    int ExecutingIndex,
    string StartStationName,
    int StartStationNo,
    string EndStationName,
    int EndStationNo,
    string ExecuteVehicleName,
    string ExecuteVehicleKey,
    string TaskState,
    string Source,
    string FailReason,
    string StartEndStationNameDetail,
    string CreatedBy,
    string ModifiedBy,
    DateTime CreateTime,
    DateTime UpdateTime,
    DateTime? DoneTime,
    List<OrderMissionDto> Missions
);


public class OrderMissionDto
{
    public int Id { get; set; }

    [JsonPropertyName("missionState")]
    public int MissionState { get; set; }

    public int ExecutingIndex { get; set; }
    public string Type { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    [JsonPropertyName("destination")]
    public int Destination { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    public string MapName { get; set; } = string.Empty;
    public int ResultCode { get; set; }
    public string ResultStr { get; set; } = string.Empty;
    public DateTime? CreateTime { get; set; }
    public DateTime? ExecuteTime { get; set; }
    public DateTime? FinishTime { get; set; }
}

