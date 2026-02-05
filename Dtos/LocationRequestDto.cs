using WorkOrderApplication.API.Enums;

namespace WorkOrderApplication.API.Dtos;

/// <summary>
/// DTO สำหรับการสร้าง ShipmentProcess
/// </summary>
public class LocationRequestDto
{
    public required string SourceStation { get; init; }
    public required string DestinationStation { get; init; }
    public required int OrderProcessId { get; init; }
    public ShipmentMode Mode { get; init; } = ShipmentMode.ExternalApi;  // Default = External API
    public int? UserId { get; init; } // Optional: For Manual Mode user assignment
}
