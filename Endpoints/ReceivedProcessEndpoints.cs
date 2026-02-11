using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.Results;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Mappings;
using WorkOrderApplication.API.Services; // ✅ เพิ่ม
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WorkOrderApplication.API.Hubs;

namespace WorkOrderApplication.API.Endpoints;

public static class ReceivedProcessEndpoints
{
    public static RouteGroupBuilder MapReceivedProcessEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET ALL --------------------
        group.MapGet("/", async (AppDbContext db) =>
        {
            var processes = await db.ReceivedProcesses
                .Include(rp => rp.ReceivedBy)
                .Include(rp => rp.ReceivedMaterials).ThenInclude(rm => rm.Material)
                .ToListAsync();

            return Results.Ok(processes.Select(rp => rp.ToListDto()));
        })
        .WithName("GetAllReceivedProcesses")
        .WithSummary("Get all received processes")
        .Produces<List<ReceivedProcessListDto>>(StatusCodes.Status200OK);

        // -------------------- GET BY ID --------------------
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var process = await db.ReceivedProcesses
                .Include(rp => rp.ReceivedBy)
                .Include(rp => rp.ReceivedMaterials).ThenInclude(rm => rm.Material)
                .FirstOrDefaultAsync(rp => rp.Id == id);

            return process is null ? Results.NotFound() : Results.Ok(process.ToDetailsDto());
        })
        .WithName("GetReceivedProcessById")
        .WithSummary("Get received process by Id")
        .Produces<ReceivedProcessDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // -------------------- POST --------------------
        group.MapPost("/", async (
            ReceivedProcessUpsertDto dto,
            AppDbContext db,
            IValidator<ReceivedProcessUpsertDto> validator,
            VehicleProxyService vehicleService,
            OrderProcessNotifier notifier,     // ✅ ใช้ Notifier แทน Hub
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("ReceivedProcess");

            // ✅ Validate DTO
            ValidationResult validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            // ✅ หา OrderProcess พร้อม ShipmentProcess
            var orderProcess = await db.OrderProcesses
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess)
                .Include(op => op.WorkOrder)
                .Include(op => op.CreatedBy)
                .FirstOrDefaultAsync(op => op.Id == dto.OrderProcessId);

            if (orderProcess is null)
                return Results.NotFound($"OrderProcess {dto.OrderProcessId} not found");

            // ✅ ตรวจว่ามี ShipmentProcess ไหม
            var vehicleKey = orderProcess.ShipmentProcess?.ExecuteVehicleKey;
            if (string.IsNullOrEmpty(vehicleKey))
            {
                logger.LogWarning("⚠️ No vehicleKey found in ShipmentProcess for OrderProcessId {Id}", dto.OrderProcessId);
            }

            // ✅ สร้าง ReceivedProcess entity
            var entity = dto.ToEntity();
            entity.ReceivedDate = DateTime.UtcNow;

            db.ReceivedProcesses.Add(entity);

            // ✅ อัปเดตสถานะ OrderProcess
            orderProcess.Status = "Delivered";
            await db.SaveChangesAsync();

            // ✅ ถ้ามี vehicleKey → เรียก Vehicle Pass API
            if (!string.IsNullOrEmpty(vehicleKey))
            {
                logger.LogInformation("🚗 Calling Vehicle Pass API for {VehicleKey}", vehicleKey);
                var result = await vehicleService.PassVehicleAsync(vehicleKey);

                if (result == null)
                    logger.LogWarning("❌ Failed to call PassVehicleAsync for {VehicleKey}", vehicleKey);
                else
                    logger.LogInformation("✅ Vehicle pass executed successfully: {Result}", result);
            }

            // ✅ โหลดข้อมูลใหม่ (รวม OrderProcess)
            var updatedOrderProcess = await db.OrderProcesses
                .Include(op => op.WorkOrder)
                .Include(op => op.CreatedBy)
                .Include(op => op.ConfirmProcess)
                .Include(op => op.PreparingProcess)
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess)
                .Include(op => op.CancelledProcess)
                .Include(op => op.ReturnProcess)
                .FirstOrDefaultAsync(op => op.Id == orderProcess.Id);

            var orderDto = updatedOrderProcess!.ToDetailsDto();

            // ✅ Broadcast ทั้งชุด (ตาม pattern ใหม่)
            await notifier.BroadcastReceivedCreatedAsync(orderProcess.Id, entity.ToDetailsDto());   // 🎯 ส่งเฉพาะ ReceivedProcess
            await notifier.BroadcastUpdatedAsync(orderProcess.Id, orderDto);              // 🎯 ส่ง OrderProcess ทั้งชุด

            logger.LogInformation("📡 Broadcasted ReceivedCreated & OrderProcessUpdated for OrderProcessId={Id}", orderProcess.Id);

            // ✅ ส่ง response กลับ
            return Results.Created($"/api/receivedprocesses/{entity.Id}", new
            {
                Message = "ReceivedProcess created successfully and vehicle pass executed",
                VehicleKey = vehicleKey,
                ShortageReason = entity.ShortageReason, // ✅ เพิ่มเหตุผลกลับไปให้ client
                OrderProcess = orderDto
            });
        })
        .WithName("CreateReceivedProcess")
        .WithSummary("Create received process, call vehicle pass (from ShipmentProcess), and broadcast via SignalR")
        .WithDescription("สร้าง ReceivedProcess, เรียก external API pass vehicle, และ Broadcast การอัปเดตผ่าน SignalR")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);



        // -------------------- PUT --------------------
        group.MapPut("/{id:int}", async (
            int id,
            ReceivedProcessUpsertDto dto,
            AppDbContext db,
            IValidator<ReceivedProcessUpsertDto> validator,
            OrderProcessNotifier notifier   // ✅ ใช้ notifier
        ) =>
        {
            ValidationResult validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            var entity = await db.ReceivedProcesses
                .Include(rp => rp.ReceivedMaterials)
                .FirstOrDefaultAsync(rp => rp.Id == id);

            if (entity is null)
                return Results.NotFound();

            // ✅ อัปเดตข้อมูลใน entity
            entity.UpdateEntity(dto);
            await db.SaveChangesAsync();

            // ✅ โหลด OrderProcess รวมทุก process เพื่อ Broadcast
            var orderProcess = await db.OrderProcesses
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
                .Include(op => op.ConfirmProcess)
                .Include(op => op.PreparingProcess)
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess)
                .Include(op => op.CancelledProcess)
                .Include(op => op.ReturnProcess)
                .FirstOrDefaultAsync(op => op.Id == entity.OrderProcessId);

            if (orderProcess is not null)
            {
                // ✅ Broadcast เฉพาะ ReceivedProcess
                await notifier.BroadcastReceivedUpdatedAsync(orderProcess.Id, entity.ToDetailsDto());

                // ✅ Broadcast ทั้ง OrderProcess
                await notifier.BroadcastUpdatedAsync(orderProcess.Id, orderProcess.ToDetailsDto());
            }

            // ✅ โหลดข้อมูล ReceivedProcess ที่อัปเดต เพื่อส่งกลับ
            var updated = await db.ReceivedProcesses
                .Include(rp => rp.ReceivedBy)
                .Include(rp => rp.ReceivedMaterials).ThenInclude(rm => rm.Material)
                .FirstOrDefaultAsync(rp => rp.Id == id);

            return Results.Ok(updated!.ToDetailsDto());
        })
        .WithName("UpdateReceivedProcess")
        .WithSummary("Update received process")
        .WithDescription("แก้ไข ReceivedProcess และ Broadcast การเปลี่ยนแปลงผ่าน SignalR")
        .Produces<ReceivedProcessDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);


        // -------------------- DELETE --------------------
        group.MapDelete("/{id:int}", async (
            int id,
            AppDbContext db,
            OrderProcessNotifier notifier   // ✅ เพิ่ม
        ) =>
        {
            var entity = await db.ReceivedProcesses.FindAsync(id);
            if (entity is null)
                return Results.NotFound();

            db.ReceivedProcesses.Remove(entity);

            // ✅ หา OrderProcess ที่เกี่ยวข้อง เพื่อเปลี่ยนสถานะกลับ
            var orderProcess = await db.OrderProcesses
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
                .Include(op => op.ConfirmProcess)
                .Include(op => op.PreparingProcess)
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess)
                .Include(op => op.CancelledProcess)
                .Include(op => op.ReturnProcess)
                .FirstOrDefaultAsync(op => op.Id == entity.OrderProcessId);

            if (orderProcess is not null)
            {
                // 🔙 ย้อนกลับสถานะ
                orderProcess.Status = "Shipment";
                await db.SaveChangesAsync();

                // ✅ Broadcast Real-time
                await notifier.BroadcastReceivedDeletedAsync(orderProcess.Id, entity.Id);           // แจ้งเฉพาะ ReceivedProcess ที่ถูกลบ
                await notifier.BroadcastUpdatedAsync(orderProcess.Id, orderProcess.ToDetailsDto()); // แจ้งอัปเดต OrderProcess ทั้งชุด
            }

            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteReceivedProcess")
        .WithSummary("Delete received process")
        .WithDescription("ลบ ReceiveProcess, อัปเดต OrderProcess.Status = Shipment และ Broadcast ผ่าน SignalR")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
