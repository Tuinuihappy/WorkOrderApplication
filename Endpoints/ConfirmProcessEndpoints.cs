using Microsoft.EntityFrameworkCore;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Mappings;
using WorkOrderApplication.API.Services;

namespace WorkOrderApplication.API.Endpoints;

public static class ConfirmProcessEndpoints
{
    public static RouteGroupBuilder MapConfirmProcessEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET /api/orderprocesses/{orderProcessId}/confirmprocesses --------------------
        group.MapGet("/", async (int orderProcessId, AppDbContext db) =>
        {
            var items = await db.ConfirmProcesses
                .AsNoTracking()
                .Where(c => c.OrderProcessId == orderProcessId)
                .ToListAsync();
            return Results.Ok(items.Select(c => c.ToListDto()));
        });

        // -------------------- GET /api/orderprocesses/{orderProcessId}/confirmprocesses/{id} --------------------
        group.MapGet("/{id:int}", async (int orderProcessId, int id, AppDbContext db) =>
        {
            var entity = await db.ConfirmProcesses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.OrderProcessId == orderProcessId);
            return entity is not null
                ? Results.Ok(entity.ToDetailsDto())
                : Results.NotFound();
        });

        // -------------------- POST /api/orderprocesses/{orderProcessId}/confirmprocesses --------------------
        group.MapPost("/", async (
            int orderProcessId,
            AppDbContext db,
            OrderProcessNotifier notifier,
            ILoggerFactory loggerFactory
        ) =>
        {
            var _logger = loggerFactory.CreateLogger("ConfirmProcess");

            // ✅ สร้าง ConfirmProcess โดยใช้ orderProcessId จาก URL
            var entity = new Entities.ConfirmProcess
            {
                OrderProcessId = orderProcessId,
                ConfirmedDate = DateTime.UtcNow
            };
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
                .FirstOrDefaultAsync(op => op.Id == orderProcessId);

            if (orderProcess is null)
                return Results.NotFound($"OrderProcess {orderProcessId} not found.");

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

            return Results.Created($"/api/orderprocesses/{orderProcessId}/confirmprocesses/{entity.Id}", confirmDto);
        })
        .WithName("CreateConfirmProcess")
        .WithSummary("Create confirm process")
        .WithDescription("สร้าง ConfirmProcess และอัปเดต OrderProcess.Status = Preparing แล้ว Broadcast ผ่าน SignalR")
        .Produces<ConfirmProcessDetailsDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);



        // -------------------- PUT /api/orderprocesses/{orderProcessId}/confirmprocesses/{id} --------------------
        group.MapPut("/{id:int}", async (
            int orderProcessId,
            int id,
            AppDbContext db,
            OrderProcessNotifier notifier,
            ILoggerFactory loggerFactory
        ) =>
        {
            var _logger = loggerFactory.CreateLogger("ConfirmProcess");

            // ✅ หา entity เดิม
            var entity = await db.ConfirmProcesses.FirstOrDefaultAsync(c => c.Id == id && c.OrderProcessId == orderProcessId);
            if (entity is null)
                return Results.NotFound($"ConfirmProcess {id} not found for OrderProcess {orderProcessId}.");

            // ✅ อัปเดต ConfirmedDate
            entity.ConfirmedDate = DateTime.UtcNow;

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
                .FirstOrDefaultAsync(op => op.Id == orderProcessId);

            if (orderProcess is null)
                return Results.NotFound($"OrderProcess {orderProcessId} not found.");

            // ✅ อัปเดตสถานะของ OrderProcess
            orderProcess.Status = "Preparing";
            await db.SaveChangesAsync();

            // ✅ เตรียม DTO สำหรับ broadcast
            var orderDto = orderProcess.ToDetailsDto();
            var confirmDto = entity.ToDetailsDto();

            // ✅ Broadcast ทั้งภาพรวมและย่อย
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



        // -------------------- DELETE /api/orderprocesses/{orderProcessId}/confirmprocesses/{id} --------------------
        group.MapDelete("/{id:int}", async (
            int orderProcessId,
            int id,
            AppDbContext db,
            OrderProcessNotifier notifier,
            ILoggerFactory loggerFactory
        ) =>
        {
            var _logger = loggerFactory.CreateLogger("ConfirmProcess");

            // ✅ หา ConfirmProcess ที่จะลบ
            var entity = await db.ConfirmProcesses.FirstOrDefaultAsync(c => c.Id == id && c.OrderProcessId == orderProcessId);
            if (entity is null)
                return Results.NotFound($"ConfirmProcess {id} not found for OrderProcess {orderProcessId}.");

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
                .FirstOrDefaultAsync(op => op.Id == orderProcessId);

            if (orderProcess is null)
                return Results.NotFound($"OrderProcess {orderProcessId} not found.");

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

