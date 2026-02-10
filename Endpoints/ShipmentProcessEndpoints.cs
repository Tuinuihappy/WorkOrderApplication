using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.Results;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Enums;
using WorkOrderApplication.API.Mappings;
using WorkOrderApplication.API.Services;
using Microsoft.AspNetCore.SignalR;
using WorkOrderApplication.API.Hubs;
using System.Text.Json;

namespace WorkOrderApplication.API.Endpoints;

public static class ShipmentProcessEndpoints
{
    public static RouteGroupBuilder MapShipmentProcessEndpoints(this RouteGroupBuilder group)
    {

        // -------------------- POST: /api/proxy/shipmentProcess ----------------------------------------
        group.MapPost("/shipmentProcess", async (
            LocationRequestDto dto,
            AppDbContext db,
            OrderProxyService service,
            IHubContext<ShipmentProcessHub> trackedHub,
            ILoggerFactory loggerFactory) =>
        {
            var _logger = loggerFactory.CreateLogger("ShipmentProcess");

            // 🔀 แยก Logic ตาม ShipmentMode
            if (dto.Mode == ShipmentMode.Manual)
            {
                // 🔹 Manual Mode: ไม่ต้องเช็ค OrderGroupAMR และไม่เรียก External API
                _logger.LogInformation("[Manual Mode] Creating shipment without checking OrderGroupAMR or calling External API");

                // 👤 ถ้ามี UserId ให้ดึงชื่อคนส่งมาใส่ใน ExecuteVehicleName
                string? executeVehicleName = null;
                if (dto.UserId.HasValue)
                {
                    var user = await db.Users.FindAsync(dto.UserId.Value);
                    if (user != null)
                    {
                        executeVehicleName = user.UserName;
                        _logger.LogInformation("[Manual Mode] Assigned ExecuteVehicleName = {UserName} from UserId={UserId}", user.UserName, dto.UserId);
                    }
                    else
                    {
                        _logger.LogWarning("[Manual Mode] User with Id={UserId} not found", dto.UserId);
                    }
                }

                var shipment = new ShipmentProcess
                {
                    ShipmentMode = ShipmentMode.Manual,
                    // กรณี Manual ให้เป็น 0 ไปเลย เพราะไม่จำเป็นต้อง map กับ OrderGroupAMR
                    SourceStationId = 0, 
                    DestinationStationId = 0,
                    OrderGroupId = 0,
                    
                    // รับค่าจาก UI โดยตรง
                    SourceStation = dto.SourceStation,
                    DestinationStation = dto.DestinationStation,
                    
                    OrderProcessId = dto.OrderProcessId,
                    LastSynced = DateTime.UtcNow,
                    ExecuteVehicleName = executeVehicleName, 
                };

                db.ShipmentProcesses.Add(shipment);
                await db.SaveChangesAsync();

                // 📡 Broadcast SignalR event
                await trackedHub.Clients.All.SendAsync("ShipmentProcessAdded", new
                {
                    shipment.Id,
                    shipment.ShipmentMode,
                    shipment.SourceStation,
                    shipment.SourceStationId,
                    shipment.DestinationStation,
                    shipment.DestinationStationId,
                    shipment.OrderGroupId,
                    shipment.OrderProcessId,
                    shipment.LastSynced,
                    shipment.ExecuteVehicleName,
                    Mode = "Manual"
                });

                _logger.LogInformation("[SignalR ▶️] Broadcasted ShipmentProcessAdded (Manual) for Id={Id}", shipment.Id);

                return Results.Ok(new
                {
                    id = shipment.Id,
                    mode = "Manual",
                    sourceStation = shipment.SourceStation,
                    destinationStation = shipment.DestinationStation,
                    orderProcessId = shipment.OrderProcessId,
                    message = "Manual shipment created successfully"
                });
            }
            else
            {
                // 🔹 External API Mode: ต้องเช็ค OrderGroupAMR และเรียก AMR API
                
                // ✅ หา mapping จากตาราง OrderGroupAMR (ย้ายมาทำใน else block)
                var mapping = await db.OrderGroupAMRs
                    .FirstOrDefaultAsync(x =>
                        x.SourceStation == dto.SourceStation &&
                        x.DestinationStation == dto.DestinationStation);

                if (mapping is null)
                {
                    return Results.BadRequest(new
                    {
                        error = $"No mapping found for route {dto.SourceStation} → {dto.DestinationStation}"
                    });
                }

                _logger.LogInformation("[External API Mode] Calling External API for AMR");

                var orderGroupDto = new OrderGroupRequestDto(mapping.OrderGroupId);
                var result = await service.AddOrderGroupAsync(orderGroupDto);

                using var jsonDoc = JsonDocument.Parse(result);
                var root = jsonDoc.RootElement.GetProperty("result");

                // ✅ ดึงข้อมูลจาก response
                var externalId = root.GetProperty("id").GetInt32();
                var orderId = root.GetProperty("orderId").GetString();
                var orderName = root.GetProperty("orderName").GetString();

                string? executeVehicleName = null;
                string? executeVehicleKey = null;

                if (root.TryGetProperty("executeVehicleName", out var nameProp))
                    executeVehicleName = nameProp.GetString();

                if (root.TryGetProperty("executeVehicleKey", out var keyProp))
                    executeVehicleKey = keyProp.GetString();

                // ✅ ตรวจสอบว่ามี ShipmentProcess อยู่แล้วหรือไม่ (ตาม ExternalId)
                var existing = await db.ShipmentProcesses
                    .FirstOrDefaultAsync(x => x.ExternalId == externalId);

                if (existing is null)
                {
                    // ➕ เพิ่ม ShipmentProcess ใหม่
                    var shipment = new ShipmentProcess
                    {
                        ShipmentMode = ShipmentMode.ExternalApi,
                        SourceStationId = mapping.SourceStationId,
                        SourceStation = dto.SourceStation,
                        DestinationStationId = mapping.DestinationStationId,
                        DestinationStation = dto.DestinationStation,
                        OrderGroupId = mapping.OrderGroupId,
                        ExternalId = externalId,
                        OrderId = orderId ?? "",
                        OrderName = orderName ?? "",
                        ExecuteVehicleName = executeVehicleName ?? "",
                        ExecuteVehicleKey = executeVehicleKey ?? "",
                        LastSynced = DateTime.UtcNow,
                        OrderProcessId = dto.OrderProcessId
                    };

                    db.ShipmentProcesses.Add(shipment);
                    await db.SaveChangesAsync();

                    // 📡 แจ้ง SignalR event
                    await trackedHub.Clients.All.SendAsync("ShipmentProcessAdded", new
                    {
                        shipment.Id,
                        shipment.ShipmentMode,
                        shipment.ExternalId,
                        shipment.OrderId,
                        shipment.OrderName,
                        shipment.SourceStation,
                        shipment.SourceStationId,
                        shipment.DestinationStation,
                        shipment.DestinationStationId,
                        shipment.OrderGroupId,
                        shipment.ExecuteVehicleName,
                        shipment.ExecuteVehicleKey,
                        shipment.LastSynced,
                        Mode = "AMR"
                    });

                    _logger.LogInformation("[SignalR ▶️] Broadcasted ShipmentProcessAdded (External API) for {OrderName} ({ExternalId})",
                        shipment.OrderName, shipment.ExternalId);
                }
                else
                {
                    // 🔄 ถ้ามีอยู่แล้ว → อัปเดตข้อมูล
                    existing.ExecuteVehicleName = executeVehicleName ?? existing.ExecuteVehicleName;
                    existing.ExecuteVehicleKey = executeVehicleKey ?? existing.ExecuteVehicleKey;
                    existing.LastSynced = DateTime.UtcNow;

                    await db.SaveChangesAsync();

                    await trackedHub.Clients.All.SendAsync("ShipmentProcessUpdated", new
                    {
                        existing.ExternalId,
                        existing.ExecuteVehicleName,
                        existing.ExecuteVehicleKey,
                        existing.LastSynced
                    });

                    _logger.LogInformation("[SignalR 🔄] Broadcasted ShipmentProcessUpdated for {OrderName} ({ExternalId})",
                        existing.OrderName, existing.ExternalId);
                }

                // ✅ ส่งต่อ response ดิบกลับไปให้ client
                return Results.Json(JsonSerializer.Deserialize<object>(result));
            }
        });
        
        // // -------------------- PATCH /api/shipmentprocesses/{id}/arrived --------------------
        // group.MapPatch("/{id:int}/arrived", async (
        //     int id,
        //     AppDbContext db,
        //     ILoggerFactory loggerFactory,
        //     IHubContext<ShipmentProcessHub> hubContext) =>
        // {
        //     var _logger = loggerFactory.CreateLogger("ShipmentProcess");

        //     // 🔍 หา ShipmentProcess พร้อม OrderProcess ที่เกี่ยวข้อง
        //     var shipment = await db.ShipmentProcesses
        //         .Include(s => s.OrderProcess)
        //         .FirstOrDefaultAsync(s => s.Id == id);

        //     if (shipment is null)
        //     {
        //         return Results.NotFound(new { error = $"ShipmentProcess with Id {id} not found." });
        //     }

        //     if (shipment.OrderProcess is null)
        //     {
        //         return Results.BadRequest(new { error = $"ShipmentProcess {id} is not linked to any OrderProcess." });
        //     }

        //     // ✅ ตั้งเวลา ArrivalTime โดยใช้เวลาปัจจุบัน (ไม่ต้องส่งจาก client)
        //     shipment.ArrivalTime = DateTime.UtcNow;

        //     // ✅ อัปเดทสถานะของ OrderProcess เป็น Awaiting Pickup
        //     shipment.OrderProcess.Status = "Awaiting Pickup";

        //     await db.SaveChangesAsync();

        //     // 📡 แจ้ง UI ผ่าน SignalR
        //     await hubContext.Clients.All.SendAsync("ShipmentArrived", new
        //     {
        //         shipment.Id,
        //         shipment.OrderName,
        //         shipment.ArrivalTime,
        //         shipment.OrderProcess.Status
        //     });

        //     _logger.LogInformation("[Shipment ✅] ShipmentProcess {Id} marked as Arrived (OrderProcessId={OrderProcessId})",
        //         shipment.Id, shipment.OrderProcess.Id);

        //     return Results.Ok(new
        //     {
        //         Message = $"ShipmentProcess #{shipment.Id} marked as arrived.",
        //         shipment.OrderProcess.Id,
        //         shipment.OrderProcess.Status,
        //         shipment.ArrivalTime
        //     });
        // })
        // .WithName("MarkShipmentAsArrived")
        // .WithSummary("Mark shipment as arrived (auto set ArrivalTime)")
        // .WithDescription("อัปเดท OrderProcess.Status = 'Awaiting Pickup' และเก็บเวลา ArrivalTime เป็นเวลาปัจจุบัน")
        // .Produces(StatusCodes.Status200OK)
        // .Produces(StatusCodes.Status404NotFound)
        // .Produces(StatusCodes.Status400BadRequest);
        
        // -------------------- PATCH /api/shipmentprocesses/{id}/arrived --------------------
        group.MapPatch("/{id:int}/arrived", async (
            int id,
            AppDbContext db,
            ILoggerFactory loggerFactory,
            OrderProcessNotifier notifier // ✅ ใช้ Notifier แทน HubContext
        ) =>
        {
            var _logger = loggerFactory.CreateLogger("ShipmentProcess");

            // 🔍 หา ShipmentProcess พร้อม OrderProcess ที่เกี่ยวข้อง
            var shipment = await db.ShipmentProcesses
                .Include(s => s.OrderProcess)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shipment is null)
                return Results.NotFound(new { error = $"ShipmentProcess with Id {id} not found." });

            if (shipment.OrderProcess is null)
                return Results.BadRequest(new { error = $"ShipmentProcess {id} is not linked to any OrderProcess." });

            // ✅ ตั้งเวลา ArrivalTime โดยใช้เวลาปัจจุบัน
            shipment.ArrivalTime = DateTime.UtcNow;

            // ✅ อัปเดตสถานะของ OrderProcess เป็น Awaiting Pickup
            shipment.OrderProcess.Status = "Awaiting Pickup";

            await db.SaveChangesAsync();

            // ✅ โหลดข้อมูลใหม่ (รวม OrderProcess)
            var updated = await db.ShipmentProcesses
                .Include(s => s.OrderProcess)
                .FirstAsync(s => s.Id == id);

            // ✅ Broadcast ผ่าน SignalR (Notifier)
            await notifier.BroadcastShipmentArrivedAsync(updated.OrderProcess.OrderNumber, updated.ToDto());

            _logger.LogInformation("[Shipment ✅] ShipmentProcess {Id} marked as Arrived (OrderProcessId={OrderProcessId})",
                shipment.Id, shipment.OrderProcess.Id);

            return Results.Ok(updated.ToDto());
        })
        .WithName("MarkShipmentAsArrived")
        .WithSummary("Mark shipment as arrived (auto set ArrivalTime)")
        .WithDescription("อัปเดท OrderProcess.Status = 'Awaiting Pickup' และเก็บเวลา ArrivalTime เป็นเวลาปัจจุบัน")
        .Produces<ShipmentProcessDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);



        return group;
    }
}
