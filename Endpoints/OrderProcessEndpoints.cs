using Microsoft.EntityFrameworkCore;
using FluentValidation;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Mappings;
using WorkOrderApplication.API.Services;

namespace WorkOrderApplication.API.Endpoints;

public static class OrderProcessEndpoints
{
    public static RouteGroupBuilder MapOrderProcessEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET /api/orderprocesses --------------------
        group.MapGet("/", async (AppDbContext db) =>
        {
            var orderProcesses = await db.OrderProcesses.AsNoTracking()
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
                .Include(op => op.ConfirmProcess)
                .Include(op => op.PreparingProcess)
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess)
                .Include(op => op.CancelledProcess)
                .Include(op => op.ReturnProcess)
                .ToListAsync();

            return Results.Ok(orderProcesses.Select(op => op.ToListDto()));
        });

        // -------------------- GET /api/orderprocesses/{id} --------------------
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var orderProcess = await db.OrderProcesses.AsNoTracking()
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
                    .ThenInclude(wo => wo.CreatedBy)
                .Include(op => op.WorkOrder)
                    .ThenInclude(wo => wo.UpdatedBy)
                .Include(op => op.WorkOrder)
                    .ThenInclude(wo => wo.Materials)
                .Include(op => op.OrderMaterials)
                    .ThenInclude(om => om.Material)
                .Include(op => op.ConfirmProcess)
                .Include(op => op.PreparingProcess)
                    .ThenInclude(p => p!.PreparingBy)
                .Include(op => op.PreparingProcess)
                    .ThenInclude(p => p!.PreparingMaterials)
                        .ThenInclude(pm => pm.Material)
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess!)
                    .ThenInclude(r => r.ReceivedBy)
                .Include(op => op.ReceiveProcess!)
                    .ThenInclude(r => r.ReceivedMaterials)
                        .ThenInclude(rm => rm.Material)
                .Include(op => op.CancelledProcess)
                .Include(op => op.ReturnProcess)
                .FirstOrDefaultAsync(op => op.Id == id);

            return orderProcess is not null
                ? Results.Ok(orderProcess.ToDetailsDto())
                : Results.NotFound();
        });

        // -------------------- POST: /api/orderprocesses --------------------
        group.MapPost("/", async (
            OrderProcessUpsertDto dto,
            AppDbContext db,
            IValidator<OrderProcessUpsertDto> validator,
            OrderProcessNotifier notifier // ✅ เพิ่ม
        ) =>
        {
            // ✅ ตรวจสอบข้อมูลจาก DTO
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            // ✅ แปลง DTO → Entity แล้วบันทึกลงฐานข้อมูล
            var entity = dto.ToEntity();
            db.OrderProcesses.Add(entity);
            await db.SaveChangesAsync();

            // ✅ โหลดข้อมูลที่สร้างเสร็จพร้อม Include ความสัมพันธ์
            var created = await db.OrderProcesses
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
                .FirstAsync(op => op.Id == entity.Id);

            var dtoToSend = created.ToDetailsDto();

            // ✅ Broadcast ไปยัง Client แบบ Real-time ด้วย "Id" แทน "OrderNumber"
            await notifier.BroadcastCreatedAsync(created.Id, dtoToSend);

            // ✅ ส่งผลลัพธ์กลับ
            return Results.Created($"/api/orderprocesses/{entity.Id}", dtoToSend);
        });


        // -------------------- PUT /api/orderprocesses/{id} --------------------
        group.MapPut("/{id:int}", async (
            int id,
            OrderProcessUpsertDto dto,
            AppDbContext db,
            IValidator<OrderProcessUpsertDto> validator,
            OrderProcessNotifier notifier // ✅ เพิ่ม
        ) =>
        {
            // ✅ Validate ข้อมูลจาก DTO
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            // ✅ หา entity เดิมใน DB
            var entity = await db.OrderProcesses.FirstOrDefaultAsync(op => op.Id == id);
            if (entity is null)
                return Results.NotFound(new { error = $"OrderProcess with Id {id} not found." });

            // ✅ อัปเดตข้อมูล
            entity.UpdateEntity(dto);
            await db.SaveChangesAsync();

            // ✅ โหลด entity ใหม่พร้อม include ความสัมพันธ์
            var updated = await db.OrderProcesses
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
                .Include(op => op.ConfirmProcess)
                .Include(op => op.PreparingProcess)
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess)
                .FirstAsync(op => op.Id == id);

            var dtoToSend = updated.ToDetailsDto();

            // ✅ Broadcast เมื่อมีการอัปเดต โดยส่ง id แทน orderNumber
            await notifier.BroadcastUpdatedAsync(updated.Id, dtoToSend);

            return Results.Ok(dtoToSend);
        });


        // -------------------- DELETE /api/orderprocesses/{id} --------------------
        group.MapDelete("/{id:int}", async (
            int id,
            AppDbContext db,
            OrderProcessNotifier notifier // ✅ เพิ่ม
        ) =>
        {
            // ✅ หา entity ที่จะลบ
            var entity = await db.OrderProcesses.FindAsync(id);
            if (entity is null)
                return Results.NotFound(new { error = $"OrderProcess with Id {id} not found." });

            // ✅ ลบออกจากฐานข้อมูล
            db.OrderProcesses.Remove(entity);
            await db.SaveChangesAsync();

            // ✅ Broadcast การลบด้วย id
            await notifier.BroadcastDeletedAsync(id);

            // 🟢 Log event (optional)
            Console.WriteLine($"🗑️ Deleted OrderProcess Id={id}");

            return Results.NoContent();
        });

        return group;
    }
}
