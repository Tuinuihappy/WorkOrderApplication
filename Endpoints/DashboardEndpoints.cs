using Microsoft.EntityFrameworkCore;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Endpoints;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapDashboardEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET: /api/dashboard/time-summary --------------------
        group.MapGet("/time-summary", async (
            AppDbContext db,
            DateTime? startDate = null,
            DateTime? endDate = null) =>
        {
            var query = db.OrderProcesses.AsNoTracking().AsQueryable();

            if (startDate.HasValue)
            {
                // แปลงเวลาให้เป็น UTC ตามโซนเวลาของไทย (+7) เพื่อเปรียบเทียบในฐานข้อมูลได้อย่างถูกต้อง
                var startUtc = new DateTimeOffset(startDate.Value.Date, TimeSpan.FromHours(7)).UtcDateTime;
                query = query.Where(op => op.CreatedDate >= startUtc);
            }

            if (endDate.HasValue)
            {
                // ครอบคลุมจนถึงสิ้นวันของ endDate ก่อนที่จะแปลงเป็น UTC
                var endUtc = new DateTimeOffset(endDate.Value.Date, TimeSpan.FromHours(7)).AddDays(1).UtcDateTime;
                query = query.Where(op => op.CreatedDate < endUtc);
            }

            // 1. Fetch Data for Order Processes
            var processes = await query
                .Select(p => new ProcessDataDto
                {
                    Id = p.Id,
                    OrderNumber = p.OrderNumber,
                    Status = p.Status,
                    DestinationStation = p.DestinationStation,
                    CreatedDate = p.CreatedDate,
                    DefaultLine = p.WorkOrder != null ? (p.WorkOrder.DefaultLine ?? "Unknown") : "Unknown",
                    ShipmentMode = p.ShipmentProcess != null ? p.ShipmentProcess.ShipmentMode.ToString() : null
                })
                .ToListAsync();

            // 2. Fetch all DefaultLines that exist in the database (from WorkOrders)
            var allLines = await db.WorkOrders
                .Where(w => w.DefaultLine != null)
                .Select(w => w.DefaultLine)
                .Distinct()
                .ToListAsync();

            // 3. Group processes by DefaultLine
            var groupedProcesses = processes
                .GroupBy(p => p.DefaultLine)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 4. Merge all lines with processed data (to include lines with 0 orders)
            var summary = allLines
                .Select(lineName => 
                {
                    var lineOrders = groupedProcesses.ContainsKey(lineName!) ? groupedProcesses[lineName!] : new List<ProcessDataDto>();
                    return new DashboardLineSummaryDto(
                        lineName!,
                        lineOrders.Count,
                        lineOrders.Select(x => new DashboardOrderProcessDto(
                            x.Id,
                            x.OrderNumber,
                            x.Status,
                            x.DestinationStation,
                            x.CreatedDate
                        )).OrderByDescending(x => x.CreatedDate).ToList()
                    );
                })
                .OrderBy(l => l.DefaultLine)
                .ToList();

            // Also include "Unknown" group if there are processes without a mapped line
            if (groupedProcesses.ContainsKey("Unknown"))
            {
                var unknownOrders = groupedProcesses["Unknown"];
                summary.Add(new DashboardLineSummaryDto(
                    "Unknown",
                    unknownOrders.Count,
                    unknownOrders.Select(x => new DashboardOrderProcessDto(
                        x.Id,
                        x.OrderNumber,
                        x.Status,
                        x.DestinationStation,
                        x.CreatedDate
                    )).OrderByDescending(x => x.CreatedDate).ToList()
                ));
            }

            // 5. Calculate Overall Totals and Status Breakdown
            var totalOrders = processes.Count;
            var predefinedStatuses = new[] { "Order Placed", "Preparing", "In Transit", "Awaiting Pickup", "Delivered" };
            var groupedStatuses = processes.GroupBy(p => p.Status).ToDictionary(g => g.Key, g => g.Count());

            var statusCounts = predefinedStatuses.Select(status => new DashboardStatusCountDto(
                status,
                groupedStatuses.ContainsKey(status) ? groupedStatuses[status] : 0
            )).ToList();

            // Add any unexpected statuses
            var otherCounts = groupedStatuses
                .Where(kvp => !predefinedStatuses.Contains(kvp.Key))
                .Select(kvp => new DashboardStatusCountDto(kvp.Key, kvp.Value))
                .OrderBy(s => s.Status);

            statusCounts.AddRange(otherCounts);

            // 6. Calculate ShipmentMode Breakdown
            var predefinedModes = new[] { "AMR", "Manual" };
            
            // Only count those that have a ShipmentMode (i.e. have a ShipmentProcess)
            var shippedProcesses = processes.Where(p => p.ShipmentMode != null).ToList();
            
            // Rename ExternalApi to AMR
            var groupedModes = shippedProcesses
                .Select(p => p.ShipmentMode == "ExternalApi" ? "AMR" : p.ShipmentMode!)
                .GroupBy(m => m)
                .ToDictionary(g => g.Key, g => g.Count());

            var shipmentModeCounts = predefinedModes.Select(mode => new DashboardShipmentModeCountDto(
                mode,
                groupedModes.ContainsKey(mode) ? groupedModes[mode] : 0
            )).ToList();

            // Include any unexpected modes just in case
            var otherModes = groupedModes
                .Where(kvp => !predefinedModes.Contains(kvp.Key))
                .Select(kvp => new DashboardShipmentModeCountDto(kvp.Key, kvp.Value))
                .OrderBy(m => m.Mode);
                
            shipmentModeCounts.AddRange(otherModes);

            var overallSummary = new DashboardOverallSummaryDto(
                totalOrders,
                statusCounts,
                shipmentModeCounts,
                summary
            );

            return Results.Ok(overallSummary);
        })
        .WithName("GetDashboardTimeSummary")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

        return group;
    }

    public class ProcessDataDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string DestinationStation { get; set; } = default!;
        public DateTime CreatedDate { get; set; }
        public string DefaultLine { get; set; } = default!;
        public string? ShipmentMode { get; set; }
    }
}
