namespace WorkOrderApplication.API.Dtos;

// ใช้สำหรับ Insert / Update
public record OrderGroupAMRUpsertDto(
    int SourceStationId,
    string SourceStation,
    int DestinationStationId,
    string DestinationStation,
    int OrderGroupId
);

// ใช้สำหรับดึงข้อมูลเต็มของ 1 record
public record OrderGroupAMRDetailsDto(
    int Id,
    int SourceStationId,
    string SourceStation,
    int DestinationStationId,
    string DestinationStation,
    int OrderGroupId
);

// ใช้สำหรับแสดงในตาราง List
public record OrderGroupAMRListDto(
    int Id,
    string SourceStation,
    string DestinationStation,
    int OrderGroupId
);


