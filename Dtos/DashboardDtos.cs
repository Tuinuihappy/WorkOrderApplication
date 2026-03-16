namespace WorkOrderApplication.API.Dtos;

// Represents an individual order process linked to a WorkOrder
public record DashboardOrderProcessDto(
    int ProcessId,
    string OrderNumber,
    string Status,
    string DestinationStation,
    DateTime CreatedDate
);

// Represents a summary of a specific line
public record DashboardLineSummaryDto(
    string DefaultLine,
    int TotalOrders,
    IEnumerable<DashboardOrderProcessDto> Orders
);

// Represents a count of orders for a specific status
public record DashboardStatusCountDto(
    string Status,
    int Count
);

// Represents a count of orders for a specific shipment mode
public record DashboardShipmentModeCountDto(
    string Mode,
    int Count
);

// Represents the overall dashboard summary
public record DashboardOverallSummaryDto(
    int TotalOrders,
    IEnumerable<DashboardStatusCountDto> StatusCounts,
    IEnumerable<DashboardShipmentModeCountDto> ShipmentModeCounts,
    IEnumerable<DashboardLineSummaryDto> Lines
);
