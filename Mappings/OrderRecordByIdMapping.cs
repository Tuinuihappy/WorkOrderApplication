using System.Text.Json;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Mappings;

public static class OrderRecordByIdMapping
{
    // 🧩 แปลงจาก Dto → Entity (OrderRecordById)
    public static OrderRecordById ToOrderRecordById(this OrderRecordByIdDto dto)
    {
        var entity = new OrderRecordById
        {
            Id = dto.Id,
            OrderId = dto.OrderId,
            OrderName = dto.OrderName,
            OrderState = dto.OrderState,
            OrderType = dto.OrderType,
            Progress = dto.Progress,
            ExecutingIndex = dto.ExecutingIndex,
            StartStationName = dto.StartStationName,
            StartStationNo = dto.StartStationNo,
            EndStationName = dto.EndStationName,
            EndStationNo = dto.EndStationNo,
            ExecuteVehicleName = dto.ExecuteVehicleName,
            ExecuteVehicleKey = dto.ExecuteVehicleKey,
            TaskState = dto.TaskState,
            Source = dto.Source,
            FailReason = dto.FailReason,
            StartEndStationNameDetail = dto.StartEndStationNameDetail,
            CreatedBy = dto.CreatedBy,
            ModifiedBy = dto.ModifiedBy,
            CreateTime = dto.CreateTime,
            UpdateTime = dto.UpdateTime,
            DoneTime = dto.DoneTime,
            RawResponse = JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        };

        // 🧭 แปลง Mission (list) พร้อมตั้งค่า FK
        entity.Missions = dto.Missions?.Select(m =>
        {
            var mission = m.ToOrderMission();
            mission.OrderRecordByIdId = entity.Id; // ✅ เพิ่มตรงนี้
            return mission;
        }).ToList() ?? new List<OrderMission>();

        return entity;
    }

    // 🧩 แปลงจาก Dto → Entity (OrderMission)
    public static OrderMission ToOrderMission(this OrderMissionDto dto)
    {
        return new OrderMission
        {
            Id = dto.Id,
            MissionState = dto.MissionState,
            ExecutingIndex = dto.ExecutingIndex,
            Type = dto.Type,
            ActionName = dto.ActionName,
            Destination = dto.Destination,
            DestinationName = dto.DestinationName,
            MapName = dto.MapName,
            ResultCode = dto.ResultCode,
            ResultStr = dto.ResultStr,
            CreateTime = dto.CreateTime,
            ExecuteTime = dto.ExecuteTime,
            FinishTime = dto.FinishTime
        };
    }

    // 🧩 (Optional) Entity → Dto (ใช้ตอน Broadcast)
    public static OrderRecordByIdDto ToDto(this OrderRecordById entity)
    {
        return new OrderRecordByIdDto(
            entity.Id,
            entity.OrderId,
            entity.OrderName,
            entity.OrderState,
            entity.OrderType,
            entity.Progress,
            entity.ExecutingIndex,
            entity.StartStationName,
            entity.StartStationNo,
            entity.EndStationName,
            entity.EndStationNo,
            entity.ExecuteVehicleName,
            entity.ExecuteVehicleKey,
            entity.TaskState,
            entity.Source,
            entity.FailReason,
            entity.StartEndStationNameDetail,
            entity.CreatedBy,
            entity.ModifiedBy,
            entity.CreateTime,
            entity.UpdateTime,
            entity.DoneTime,
            entity.Missions?.Select(m => m.ToDto()).ToList() ?? new List<OrderMissionDto>()
        );
    }

    // 🧩 (Optional) Mission Entity → Dto
    public static OrderMissionDto ToDto(this OrderMission entity)
    {
        return new OrderMissionDto
        {
            Id = entity.Id,
            MissionState = entity.MissionState,
            ExecutingIndex = entity.ExecutingIndex,
            Type = entity.Type,
            ActionName = entity.ActionName,
            Destination = entity.Destination,
            DestinationName = entity.DestinationName,
            MapName = entity.MapName,
            ResultCode = entity.ResultCode,
            ResultStr = entity.ResultStr,
            CreateTime = entity.CreateTime,
            ExecuteTime = entity.ExecuteTime,
            FinishTime = entity.FinishTime
        };
    }
}
