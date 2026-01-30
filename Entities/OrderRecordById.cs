using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkOrderApplication.API.Entities;

public class OrderRecordById
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string OrderName { get; set; } = string.Empty;
    public int OrderState { get; set; }
    public int OrderType { get; set; }
    public double Progress { get; set; }
    public int ExecutingIndex { get; set; }

    public string StartStationName { get; set; } = string.Empty;
    public int StartStationNo { get; set; }
    public string EndStationName { get; set; } = string.Empty;
    public int EndStationNo { get; set; }

    public string ExecuteVehicleName { get; set; } = string.Empty;
    public string ExecuteVehicleKey { get; set; } = string.Empty;
    public string TaskState { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public string FailReason { get; set; } = string.Empty;
    public string StartEndStationNameDetail { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = string.Empty;

    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
    public DateTime? DoneTime { get; set; }

    // 🔎 เก็บ JSON เต็ม ๆ สำหรับ debug/trace
    public string RawResponse { get; set; } = string.Empty;

    // 🔁 Navigation
    public ICollection<OrderMission> Missions { get; set; } = new List<OrderMission>();
}

public class OrderMission
{
    public int Id { get; set; }                       // ใช้ external mission id, ไม่ gen auto
    public int OrderRecordByIdId { get; set; }        // FK → OrderRecordById
    
    public int MissionState { get; set; }
    public int ExecutingIndex { get; set; }
    public string Type { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public int Destination { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    public string MapName { get; set; } = string.Empty;
    public int ResultCode { get; set; }
    public string ResultStr { get; set; } = string.Empty;

    public DateTime? CreateTime { get; set; }
    public DateTime? ExecuteTime { get; set; }
    public DateTime? FinishTime { get; set; }

    // Navigation
    public OrderRecordById? OrderRecord { get; set; }
}