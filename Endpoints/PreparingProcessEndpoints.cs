using Microsoft.EntityFrameworkCore;
using FluentValidation;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Mappings;
using WorkOrderApplication.API.Services; // ✅ เพิ่ม SignalR Service

namespace WorkOrderApplication.API.Endpoints;

public static class PreparingProcessEndpoints
{
    public static RouteGroupBuilder MapPreparingProcessEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET: /api/preparingprocesses --------------------
        group.MapGet("/", async (AppDbContext db) =>
        {
            var processes = await db.PreparingProcesses
                .Include(p => p.PreparingBy)
                .Include(p => p.PreparingMaterials)
                    .ThenInclude(pm => pm.Material)
                .ToListAsync();

            return Results.Ok(processes.Select(p => p.ToListDto()));
        });

        // -------------------- GET: /api/preparingprocesses/{id} --------------------
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var process = await db.PreparingProcesses
                .Include(p => p.PreparingBy)
                .Include(p => p.PreparingMaterials)
                    .ThenInclude(pm => pm.Material)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (process is null)
                return Results.NotFound();

            return Results.Ok(process.ToDetailsDto());
        });

        // -------------------- POST: /api/preparingprocesses --------------------
        group.MapPost("/", async (
            PreparingProcessUpsertDto dto,
            AppDbContext db,
            IValidator<PreparingProcessUpsertDto> validator,
            OrderProcessNotifier notifier   // ✅ ใช้สำหรับ broadcast
        ) =>
        {
            // ✅ Validate
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // ✅ Map DTO → Entity
                var entity = dto.ToEntity();
                db.PreparingProcesses.Add(entity);

                // ✅ หา OrderProcess ที่เกี่ยวข้อง
                var orderProcess = await db.OrderProcesses
                    .Include(op => op.WorkOrder)
                    .Include(op => op.CreatedBy)
                    .Include(op => op.ConfirmProcess)
                    .Include(op => op.ShipmentProcess)
                    .Include(op => op.ReceiveProcess)
                    .Include(op => op.CancelledProcess)
                    .Include(op => op.ReturnProcess)
                    .FirstOrDefaultAsync(op => op.Id == dto.OrderProcessId);

                if (orderProcess is null)
                    return Results.NotFound($"OrderProcess {dto.OrderProcessId} not found.");

                // ✅ เช็คก่อนว่า OrderProcess นี้มีการสร้าง PreparingProcess ไว้แล้วหรือยัง
                if (orderProcess.PreparingProcess != null)
                {
                    await transaction.RollbackAsync();
                    return Results.Conflict(new { error = $"OrderProcess {dto.OrderProcessId} is already prepared." });
                }

                // ✅ อัปเดตสถานะ OrderProcess
                orderProcess.Status = "In Transit"; // หรือ "Ready to Ship" ตาม workflow จริงของคุณ

                // ✅ Save changes
                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ โหลดข้อมูลรวมใหม่เพื่อส่ง DTO
                var updated = await db.OrderProcesses
                    .Include(op => op.CreatedBy)
                    .Include(op => op.WorkOrder)
                    .Include(op => op.ConfirmProcess)
                    .Include(op => op.PreparingProcess!)
                        .ThenInclude(p => p.PreparingBy)
                    .Include(op => op.PreparingProcess!)
                        .ThenInclude(p => p.PreparingMaterials)
                            .ThenInclude(pm => pm.Material)
                    .Include(op => op.ShipmentProcess)
                    .Include(op => op.ReceiveProcess)
                    .Include(op => op.CancelledProcess)
                    .Include(op => op.ReturnProcess)
                    .FirstAsync(op => op.Id == orderProcess.Id);

                var orderDto = updated.ToDetailsDto();
                var preparingDto = entity.ToDetailsDto();

                // ✅ 1️⃣ Broadcast OrderProcess ทั้งชุด (แม่)
                await notifier.BroadcastUpdatedAsync(updated.Id, orderDto);

                // ✅ 2️⃣ Broadcast เฉพาะ PreparingProcess (ลูก)
                await notifier.BroadcastPreparingCreatedAsync(updated.Id, preparingDto);

                // 🧠 Logging
                Console.WriteLine($"📢 PreparingProcess created for OrderProcessId {updated.Id}");

                return Results.Created(
                    $"/api/preparingprocesses/{entity.Id}",
                    preparingDto
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        })
        .WithName("CreatePreparingProcess")
        .WithSummary("Create a new PreparingProcess")
        .WithDescription("สร้าง PreparingProcess และอัปเดต OrderProcess.Status = Shipment แล้ว Broadcast ผ่าน SignalR")
        .Produces<PreparingProcessDetailsDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);


        // -------------------- PUT: /api/preparingprocesses/{id} --------------------
        group.MapPut("/{id:int}", async (
            int id,
            PreparingProcessUpsertDto dto,
            AppDbContext db,
            IValidator<PreparingProcessUpsertDto> validator,
            OrderProcessNotifier notifier   // ✅ ใช้สำหรับ broadcast
        ) =>
        {
            // ✅ Validate input
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            // ✅ หา PreparingProcess เดิม
            var entity = await db.PreparingProcesses
                .Include(p => p.PreparingMaterials)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (entity is null)
                return Results.NotFound($"PreparingProcess {id} not found.");

            // ✅ อัปเดตค่าจาก DTO → Entity
            entity.UpdateEntity(dto);
            await db.SaveChangesAsync();

            // ✅ โหลด OrderProcess ที่เกี่ยวข้องเพื่อ broadcast
            var orderProcess = await db.OrderProcesses
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
                .Include(op => op.ConfirmProcess)
                .Include(op => op.PreparingProcess!)
                    .ThenInclude(p => p.PreparingBy)
                .Include(op => op.PreparingProcess!)
                    .ThenInclude(p => p.PreparingMaterials)
                        .ThenInclude(pm => pm.Material)
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess)
                .Include(op => op.CancelledProcess)
                .Include(op => op.ReturnProcess)
                .FirstOrDefaultAsync(op => op.Id == entity.OrderProcessId);

            if (orderProcess is not null)
            {
                var orderDto = orderProcess.ToDetailsDto();
                var preparingDto = entity.ToDetailsDto();

                // ✅ Broadcast ทั้ง OrderProcess (แม่) และ PreparingProcess (ลูก)
                await notifier.BroadcastUpdatedAsync(orderProcess.Id, orderDto);
                await notifier.BroadcastPreparingUpdatedAsync(orderProcess.Id, preparingDto);
            }

            return Results.Ok(entity.ToDetailsDto());
        })
        .WithName("UpdatePreparingProcess")
        .WithSummary("Update an existing PreparingProcess")
        .WithDescription("แก้ไข PreparingProcess และ Broadcast การเปลี่ยนแปลงผ่าน SignalR")
        .Produces<PreparingProcessDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);


       // -------------------- DELETE: /api/preparingprocesses/{id} --------------------
        group.MapDelete("/{id:int}", async (
            int id,
            AppDbContext db,
            OrderProcessNotifier notifier   // ✅ ใช้สำหรับ broadcast
        ) =>
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // ✅ หา PreparingProcess ที่จะลบ
                var entity = await db.PreparingProcesses.FirstOrDefaultAsync(p => p.Id == id);
                if (entity is null)
                    return Results.NotFound($"PreparingProcess {id} not found.");

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

                // ✅ ย้อนสถานะของ OrderProcess กลับเป็น "Preparing" (หรือ Pending ตาม workflow)
                orderProcess.Status = "Preparing";

                // ✅ ลบ PreparingProcess ออกจากฐานข้อมูล
                db.PreparingProcesses.Remove(entity);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ เตรียม DTO สำหรับ broadcast
                var orderDto = orderProcess.ToDetailsDto();

                // ✅ 1️⃣ Broadcast OrderProcess ทั้งชุด (แม่)
                await notifier.BroadcastUpdatedAsync(orderProcess.Id, orderDto);

                // ✅ 2️⃣ Broadcast เฉพาะ PreparingProcess ที่ถูกลบ (ลูก)
                await notifier.BroadcastPreparingDeletedAsync(orderProcess.Id, entity.Id);

                return Results.NoContent();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        })
        .WithName("DeletePreparingProcess")
        .WithSummary("Delete a PreparingProcess")
        .WithDescription("ลบ PreparingProcess และอัปเดต OrderProcess.Status = Preparing แล้ว Broadcast ผ่าน SignalR")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);


        return group;
    }
}
