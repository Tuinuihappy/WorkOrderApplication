using Microsoft.EntityFrameworkCore;
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
        // -------------------- GET /api/orderprocesses/{orderProcessId}/shipmentprocess --------------------
        group.MapGet("/", async (int orderProcessId, AppDbContext db) =>
        {
            var items = await db.ShipmentProcesses
                .AsNoTracking()
                .Include(s => s.OrderProcess)
                .Where(s => s.OrderProcessId == orderProcessId)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
            return Results.Ok(items.Select(s => s.ToListDto()));
        })
        .WithName("GetShipmentProcessesByOrderProcess")
        .WithSummary("Get ShipmentProcesses by OrderProcessId")
        .WithDescription("ดึงข้อมูล ShipmentProcess ทั้งหมดของ OrderProcess ที่ระบุ")
        .Produces<IEnumerable<ShipmentProcessListDto>>(StatusCodes.Status200OK);

        // -------------------- GET /api/orderprocesses/{orderProcessId}/shipmentprocess/{id} --------------------
        group.MapGet("/{id:int}", async (int orderProcessId, int id, AppDbContext db) =>
        {
            var entity = await db.ShipmentProcesses
                .AsNoTracking()
                .Include(s => s.OrderProcess)
                .FirstOrDefaultAsync(s => s.Id == id && s.OrderProcessId == orderProcessId);

            if (entity is null)
                return Results.NotFound(new { error = $"ShipmentProcess with Id {id} not found." });

            return Results.Ok(entity.ToDetailsDto());
        })
        .WithName("GetShipmentProcessByOrderProcess")
        .WithSummary("Get ShipmentProcess by Id under OrderProcess")
        .WithDescription("ดึงข้อมูล ShipmentProcess ตาม Id ภายใต้ OrderProcess ที่ระบุ")
        .Produces<ShipmentProcessDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // -------------------- POST /api/orderprocesses/{orderProcessId}/shipmentprocess --------------------
        group.MapPost("/", async (
            int orderProcessId,
            LocationRequestDto dto,
            AppDbContext db,
            OrderProxyService service,
            IHubContext<ShipmentProcessHub> trackedHub,
            OrderProcessNotifier notifier,
            ILoggerFactory loggerFactory) =>
        {
            var _logger = loggerFactory.CreateLogger("ShipmentProcess");

            // ✅ ตรวจสอบว่า OrderProcess มีอยู่จริง
            var orderProcess = await db.OrderProcesses
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
                .Include(op => op.ConfirmProcess)
                .Include(op => op.PreparingProcess)
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess)
                .Include(op => op.CancelledProcess)
                .Include(op => op.ReturnProcess)
                .FirstOrDefaultAsync(op => op.Id == orderProcessId);

            if (orderProcess is null)
                return Results.NotFound(new { error = $"OrderProcess {orderProcessId} not found." });

            // 🔀 แยก Logic ตาม ShipmentMode
            if (dto.Mode == ShipmentMode.Manual)
            {
                // 🔹 Manual Mode: ไม่ต้องเช็ค OrderGroupAMR และไม่เรียก External API
                _logger.LogInformation("[Manual Mode] Creating shipment for OrderProcessId={OrderProcessId}", orderProcessId);

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
                    SourceStationId = 0,
                    DestinationStationId = 0,
                    OrderGroupId = 0,
                    SourceStation = dto.SourceStation,
                    DestinationStation = dto.DestinationStation,
                    OrderProcessId = orderProcessId, // ✅ ใช้จาก URL แทน DTO
                    LastSynced = DateTime.UtcNow,
                    ExecuteVehicleName = executeVehicleName,
                };

                db.ShipmentProcesses.Add(shipment);

                // ✅ อัปเดตสถานะ OrderProcess เป็น "In Transit"
                orderProcess.Status = "In Transit";

                await db.SaveChangesAsync();

                // ✅ โหลด OrderProcess ใหม่พร้อม navigation properties ครบ
                var updated = await db.OrderProcesses
                    .Include(op => op.CreatedBy)
                    .Include(op => op.WorkOrder)
                    .Include(op => op.ConfirmProcess)
                    .Include(op => op.PreparingProcess)
                    .Include(op => op.ShipmentProcess)
                    .Include(op => op.ReceiveProcess)
                    .Include(op => op.CancelledProcess)
                    .Include(op => op.ReturnProcess)
                    .FirstAsync(op => op.Id == orderProcessId);

                // ✅ Broadcast OrderProcess (แม่) ผ่าน OrderProcessHub
                var orderDto = updated.ToDetailsDto();
                await notifier.BroadcastUpdatedAsync(updated.Id, orderDto);

                // ✅ Broadcast ShipmentProcess (ลูก) ผ่าน ShipmentProcessHub
                await notifier.BroadcastShipmentCreatedAsync(updated.OrderNumber, shipment.ToDto());

                _logger.LogInformation("[Manual ✅] ShipmentProcess created for OrderProcessId={OrderProcessId}, Status → In Transit", orderProcessId);

                return Results.Created(
                    $"/api/orderprocesses/{orderProcessId}/shipmentprocess/{shipment.Id}",
                    shipment.ToDetailsDto());
            }
            else
            {
                // 🔹 External API Mode: ต้องเช็ค OrderGroupAMR และเรียก AMR API
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

                _logger.LogInformation("[External API Mode] Calling External API for AMR, OrderProcessId={OrderProcessId}", orderProcessId);

                var orderGroupDto = new OrderGroupRequestDto(mapping.OrderGroupId);
                var result = await service.AddOrderGroupAsync(orderGroupDto);

                using var jsonDoc = JsonDocument.Parse(result);
                var root = jsonDoc.RootElement.GetProperty("result");

                var externalId = root.GetProperty("id").GetInt32();
                var orderId = root.GetProperty("orderId").GetString();
                var orderName = root.GetProperty("orderName").GetString();

                string? executeVehicleName = null;
                string? executeVehicleKey = null;

                if (root.TryGetProperty("executeVehicleName", out var nameProp))
                    executeVehicleName = nameProp.GetString();

                if (root.TryGetProperty("executeVehicleKey", out var keyProp))
                    executeVehicleKey = keyProp.GetString();

                var existing = await db.ShipmentProcesses
                    .FirstOrDefaultAsync(x => x.ExternalId == externalId);

                if (existing is null)
                {
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
                        OrderProcessId = orderProcessId // ✅ ใช้จาก URL แทน DTO
                    };

                    db.ShipmentProcesses.Add(shipment);

                    // ✅ อัปเดตสถานะ OrderProcess เป็น "In Transit"
                    orderProcess.Status = "In Transit";

                    await db.SaveChangesAsync();

                    // ✅ โหลด OrderProcess ใหม่
                    var updated = await db.OrderProcesses
                        .Include(op => op.CreatedBy)
                        .Include(op => op.WorkOrder)
                        .Include(op => op.ConfirmProcess)
                        .Include(op => op.PreparingProcess)
                        .Include(op => op.ShipmentProcess)
                        .Include(op => op.ReceiveProcess)
                        .Include(op => op.CancelledProcess)
                        .Include(op => op.ReturnProcess)
                        .FirstAsync(op => op.Id == orderProcessId);

                    // ✅ Broadcast OrderProcess (แม่)
                    var orderDto = updated.ToDetailsDto();
                    await notifier.BroadcastUpdatedAsync(updated.Id, orderDto);

                    // ✅ Broadcast ShipmentProcess (ลูก)
                    await notifier.BroadcastShipmentCreatedAsync(updated.OrderNumber, shipment.ToDto());

                    _logger.LogInformation("[External API ✅] ShipmentProcess created for OrderProcessId={OrderProcessId}, Status → In Transit",
                        orderProcessId);
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

                    _logger.LogInformation("[SignalR 🔄] ShipmentProcessUpdated for {OrderName} ({ExternalId})",
                        existing.OrderName, existing.ExternalId);
                }

                return Results.Json(JsonSerializer.Deserialize<object>(result));
            }
        })
        .WithName("CreateShipmentProcess")
        .WithSummary("Create ShipmentProcess under OrderProcess")
        .WithDescription("สร้าง ShipmentProcess และอัปเดต OrderProcess.Status = 'In Transit' แล้ว Broadcast ผ่าน SignalR")
        .Produces<ShipmentProcessDetailsDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // -------------------- PATCH /api/orderprocesses/{orderProcessId}/shipmentprocess/{id}/arrived --------------------
        group.MapPatch("/{id:int}/arrived", async (
            int orderProcessId,
            int id,
            AppDbContext db,
            ILoggerFactory loggerFactory,
            OrderProcessNotifier notifier
        ) =>
        {
            var _logger = loggerFactory.CreateLogger("ShipmentProcess");

            // 🔍 หา ShipmentProcess พร้อม OrderProcess ที่เกี่ยวข้อง
            var shipment = await db.ShipmentProcesses
                .Include(s => s.OrderProcess)
                .FirstOrDefaultAsync(s => s.Id == id && s.OrderProcessId == orderProcessId);

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
