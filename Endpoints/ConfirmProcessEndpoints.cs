using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.Results;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Mappings;
using WorkOrderApplication.API.Services; // ✅ เพิ่ม

namespace WorkOrderApplication.API.Endpoints;

public static class ConfirmProcessEndpoints
{
    public static RouteGroupBuilder MapConfirmProcessEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET /api/confirmprocesses --------------------
        group.MapGet("/", async (AppDbContext db) =>
        {
            var items = await db.ConfirmProcesses.AsNoTracking().ToListAsync();
            return Results.Ok(items.Select(c => c.ToListDto()));
        });

        // -------------------- GET /api/confirmprocesses/{id} --------------------
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.ConfirmProcesses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            return entity is not null
                ? Results.Ok(entity.ToDetailsDto())
                : Results.NotFound();
        });

        // -------------------- POST /api/confirmprocesses --------------------
        group.MapPost("/", async (
            ConfirmProcessUpsertDto dto,
            AppDbContext db,
            IValidator<ConfirmProcessUpsertDto> validator,
            OrderProcessNotifier notifier,   // ✅ ใช้สำหรับ broadcast
            ILoggerFactory loggerFactory
        ) =>
        {
            var _logger = loggerFactory.CreateLogger("ConfirmProcess");

            // ✅ Validate ข้อมูลจาก DTO
            ValidationResult validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            // ✅ แปลง DTO -> Entity แล้วเพิ่มเข้า DbContext
            var entity = dto.ToEntity();
            db.ConfirmProcesses.Add(entity);

            // ✅ หา OrderProcess ที่เกี่ยวข้อง
            var orderProcess = await db.OrderProcesses
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
                .Include(op => op.PreparingProcess)
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess)
                .Include(op => op.CancelledProcess)
                .Include(op => op.ReturnProcess)
                .FirstOrDefaultAsync(op => op.Id == dto.OrderProcessId);

            if (orderProcess is null)
                return Results.NotFound($"OrderProcess {dto.OrderProcessId} not found.");

            // ✅ อัปเดตสถานะของ OrderProcess
            orderProcess.Status = "Preparing";
            await db.SaveChangesAsync();

            // ✅ โหลด OrderProcess ใหม่ (พร้อม navigation properties ครบ)
            var updated = await db.OrderProcesses
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
                .Include(op => op.ConfirmProcess)
                .Include(op => op.PreparingProcess)
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess)
                .Include(op => op.CancelledProcess)
                .Include(op => op.ReturnProcess)
                .FirstAsync(op => op.Id == orderProcess.Id);

            // ✅ สร้าง DTO เพื่อส่ง broadcast
            var orderDto = updated.ToDetailsDto();
            var confirmDto = entity.ToDetailsDto();

            // ✅ 1️⃣ Broadcast OrderProcess ทั้งชุด (แม่)
            await notifier.BroadcastUpdatedAsync(updated.Id, orderDto);

            // ✅ 2️⃣ Broadcast เฉพาะ ConfirmProcess ที่สร้างใหม่ (ลูก)
            await notifier.BroadcastConfirmCreatedAsync(updated.Id, confirmDto);

            // 🧠 Logging
            _logger.LogInformation("📢 ConfirmProcess created for OrderProcessId {Id}", updated.Id);

            return Results.Created($"/api/confirmprocesses/{entity.Id}", confirmDto);
        })
        .WithName("CreateConfirmProcess")
        .WithSummary("Create confirm process")
        .WithDescription("สร้าง ConfirmProcess และอัปเดต OrderProcess.Status = Preparing แล้ว Broadcast ผ่าน SignalR")
        .Produces<ConfirmProcessDetailsDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);



        // -------------------- PUT /api/confirmprocesses/{id} --------------------
        group.MapPut("/{id:int}", async (
            int id,
            ConfirmProcessUpsertDto dto,
            AppDbContext db,
            IValidator<ConfirmProcessUpsertDto> validator,
            OrderProcessNotifier notifier,
            ILoggerFactory loggerFactory
        ) =>
        {
            var _logger = loggerFactory.CreateLogger("ConfirmProcess");

            // ✅ Validate input
            ValidationResult validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            // ✅ หา entity เดิม
            var entity = await db.ConfirmProcesses.FirstOrDefaultAsync(c => c.Id == id);
            if (entity is null)
                return Results.NotFound($"ConfirmProcess {id} not found.");

            // ✅ อัปเดตค่าจาก DTO → Entity
            entity.UpdateEntity(dto);

            // ✅ หา OrderProcess ที่เกี่ยวข้อง
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

            if (orderProcess is null)
                return Results.NotFound($"OrderProcess {entity.OrderProcessId} not found.");

            // ✅ อัปเดตสถานะของ OrderProcess (Confirm แล้วสถานะยังอยู่ที่ Preparing)
            orderProcess.Status = "Preparing";
            await db.SaveChangesAsync();

            // ✅ เตรียม DTO สำหรับ broadcast
            var orderDto = orderProcess.ToDetailsDto();
            var confirmDto = entity.ToDetailsDto();

            // ✅ Broadcast ทั้งภาพรวมและย่อย (ใช้ Id แทน OrderNumber)
            await notifier.BroadcastUpdatedAsync(orderProcess.Id, orderDto);
            await notifier.BroadcastConfirmUpdatedAsync(orderProcess.Id, confirmDto);

            // 🧠 Logging
            _logger.LogInformation("📢 ConfirmProcess updated for OrderProcessId {Id}", orderProcess.Id);

            return Results.Ok(confirmDto);
        })
        .WithName("UpdateConfirmProcess")
        .WithSummary("Update confirm process")
        .WithDescription("แก้ไข ConfirmProcess และ Broadcast ทั้ง OrderProcess และ ConfirmProcess ผ่าน SignalR แบบเรียลไทม์")
        .Produces<ConfirmProcessDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);



        // -------------------- DELETE /api/confirmprocesses/{id} --------------------
        group.MapDelete("/{id:int}", async (
            int id,
            AppDbContext db,
            OrderProcessNotifier notifier,   // ✅ ใช้สำหรับ broadcast
            ILoggerFactory loggerFactory
        ) =>
        {
            var _logger = loggerFactory.CreateLogger("ConfirmProcess");

            // ✅ หา ConfirmProcess ที่จะลบ
            var entity = await db.ConfirmProcesses.FirstOrDefaultAsync(c => c.Id == id);
            if (entity is null)
                return Results.NotFound($"ConfirmProcess {id} not found.");

            // ✅ หา OrderProcess ที่เกี่ยวข้อง
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

            if (orderProcess is null)
                return Results.NotFound($"OrderProcess {entity.OrderProcessId} not found.");

            // ✅ อัปเดตสถานะของ OrderProcess กลับไป Pending
            orderProcess.Status = "Pending";

            // ✅ ลบ ConfirmProcess ออกจากฐานข้อมูล
            db.ConfirmProcesses.Remove(entity);
            await db.SaveChangesAsync();

            // ✅ เตรียม DTO สำหรับ broadcast
            var orderDto = orderProcess.ToDetailsDto();

            // ✅ 1️⃣ Broadcast OrderProcess ทั้งชุด (แม่)
            await notifier.BroadcastUpdatedAsync(orderProcess.Id, orderDto);

            // ✅ 2️⃣ Broadcast เฉพาะ ConfirmProcess ที่ถูกลบ (ลูก)
            await notifier.BroadcastConfirmDeletedAsync(orderProcess.Id, entity.Id);

            // 🧠 Logging
            _logger.LogInformation("📢 ConfirmProcess deleted for OrderProcessId {Id}", orderProcess.Id);

            return Results.NoContent();
        })
        .WithName("DeleteConfirmProcess")
        .WithSummary("Delete confirm process")
        .WithDescription("ลบ ConfirmProcess และ reset OrderProcess.Status = Pending แล้ว Broadcast ทั้ง OrderProcess และ ConfirmProcess ผ่าน SignalR")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);


        return group;
    }
}

