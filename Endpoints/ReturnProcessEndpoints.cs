using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.Results;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Mappings;
using WorkOrderApplication.API.Services; // ✅ ใช้ OrderProcessNotifier

namespace WorkOrderApplication.API.Endpoints;

public static class ReturnProcessEndpoints
{
    public static RouteGroupBuilder MapReturnProcessEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET: /api/returnprocesses --------------------
        group.MapGet("/", async (AppDbContext db) =>
        {
            var list = await db.ReturnProcesses
                .Include(rp => rp.ReturnByUser)
                .AsNoTracking()
                .Select(rp => rp.ToListDto())
                .ToListAsync();

            return Results.Ok(list);
        })
        .WithName("GetReturnProcesses")
        .WithSummary("Get all ReturnProcesses")
        .Produces<List<ReturnProcessListDto>>(StatusCodes.Status200OK);

        // -------------------- GET: /api/returnprocesses/{id} --------------------
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.ReturnProcesses
                .Include(rp => rp.ReturnByUser)
                .FirstOrDefaultAsync(rp => rp.Id == id);

            return entity is null
                ? Results.NotFound()
                : Results.Ok(entity.ToDetailsDto());
        })
        .WithName("GetReturnProcessById")
        .WithSummary("Get ReturnProcess by Id")
        .Produces<ReturnProcessDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // -------------------- POST: /api/returnprocesses --------------------
        // group.MapPost("/", async (
        //     ReturnProcessUpsertDto dto,
        //     AppDbContext db,
        //     IValidator<ReturnProcessUpsertDto> validator,
        //     OrderProcessNotifier notifier  // ✅ เพิ่ม SignalR Notifier
        // ) =>
        // {
        //     // ✅ ตรวจสอบความถูกต้องของข้อมูล
        //     ValidationResult validationResult = await validator.ValidateAsync(dto);
        //     if (!validationResult.IsValid)
        //         return Results.BadRequest(validationResult.Errors);

        //     // ✅ สร้าง Entity
        //     var entity = dto.ToEntity();
        //     db.ReturnProcesses.Add(entity);

        //     // ✅ อัปเดตสถานะของ OrderProcess
        //     var orderProcess = await db.OrderProcesses
        //         .Include(op => op.CreatedBy)
        //         .Include(op => op.WorkOrder)
        //         .Include(op => op.ConfirmProcess)
        //         .Include(op => op.PreparingProcess)
        //         .Include(op => op.ShipmentProcess)
        //         .Include(op => op.ReceiveProcess)
        //         .Include(op => op.CancelledProcess)
        //         .Include(op => op.ReturnProcess)
        //         .FirstOrDefaultAsync(op => op.Id == dto.OrderProcessId);

        //     if (orderProcess is not null)
        //     {
        //         orderProcess.Status = "Returned";
        //     }

        //     // ✅ บันทึกข้อมูลทั้งหมด
        //     await db.SaveChangesAsync();

        //     // ✅ โหลดข้อมูลใหม่พร้อม Navigation
        //     var created = await db.ReturnProcesses
        //         .Include(rp => rp.ReturnByUser)
        //         .FirstOrDefaultAsync(rp => rp.Id == entity.Id);

        //     // ✅ Broadcast OrderProcess ผ่าน SignalR
        //     if (orderProcess is not null)
        //     {
        //         await notifier.BroadcastUpdatedAsync(orderProcess.OrderNumber, orderProcess.ToDetailsDto());
        //     }

        //     return Results.Created($"/api/returnprocesses/{entity.Id}", created!.ToDetailsDto());
        // })
        // .WithName("CreateReturnProcess")
        // .WithSummary("Create new ReturnProcess and broadcast OrderProcess via SignalR")
        // .WithDescription("สร้าง ReturnProcess, อัปเดต OrderProcess.Status = 'Returned' และ Broadcast แบบเรียลไทม์")
        // .Produces<ReturnProcessDetailsDto>(StatusCodes.Status201Created)
        // .Produces(StatusCodes.Status400BadRequest);

        // -------------------- POST: /api/returnprocesses --------------------
        group.MapPost("/", async (
            ReturnProcessUpsertDto dto,
            AppDbContext db,
            IValidator<ReturnProcessUpsertDto> validator,
            VehicleProxyService vehicleService,          // ✅ เพิ่ม service สำหรับเรียก external API
            OrderProcessNotifier notifier,               // ✅ ใช้สำหรับ broadcast SignalR
            ILoggerFactory loggerFactory                 // ✅ สำหรับ logging
        ) =>
        {
            var logger = loggerFactory.CreateLogger("ReturnProcess");

            // ✅ Validate DTO
            ValidationResult validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            // ✅ สร้าง Entity
            var entity = dto.ToEntity();
            db.ReturnProcesses.Add(entity);

            // ✅ หา OrderProcess และข้อมูล Shipment (เพื่อดึง vehicleKey)
            var orderProcess = await db.OrderProcesses
                .Include(op => op.ShipmentProcess)
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
                .Include(op => op.ConfirmProcess)
                .Include(op => op.PreparingProcess)
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess)
                .Include(op => op.CancelledProcess)
                .Include(op => op.ReturnProcess)
                .FirstOrDefaultAsync(op => op.Id == dto.OrderProcessId);

            if (orderProcess is null)
                return Results.NotFound($"OrderProcess {dto.OrderProcessId} not found");

            // ✅ อัปเดตสถานะ OrderProcess
            orderProcess.Status = "Returned";

            // ✅ บันทึกข้อมูลทั้งหมด
            await db.SaveChangesAsync();

            // ✅ เรียก Vehicle Pass API ถ้ามี vehicleKey
            var vehicleKey = orderProcess.ShipmentProcess?.ExecuteVehicleKey;
            if (!string.IsNullOrEmpty(vehicleKey))
            {
                logger.LogInformation("🚗 Calling Vehicle Pass API for {VehicleKey}", vehicleKey);

                var result = await vehicleService.PassVehicleAsync(vehicleKey);
                if (result == null)
                {
                    logger.LogWarning("❌ Failed to call PassVehicleAsync for {VehicleKey}", vehicleKey);
                }
                else
                {
                    logger.LogInformation("✅ Vehicle pass executed successfully: {Result}", result);
                }
            }
            else
            {
                logger.LogWarning("⚠️ No vehicleKey found in ShipmentProcess for OrderProcessId {Id}", dto.OrderProcessId);
            }

            // ✅ โหลด ReturnProcess พร้อม Navigation
            var created = await db.ReturnProcesses
                .Include(rp => rp.ReturnByUser)
                .FirstOrDefaultAsync(rp => rp.Id == entity.Id);

            // ✅ Broadcast OrderProcess ผ่าน SignalR
            await notifier.BroadcastUpdatedAsync(orderProcess.Id, orderProcess.ToDetailsDto());
            logger.LogInformation("📡 Broadcasted OrderProcessUpdated for ID {Id}", orderProcess.Id);

            // ✅ ส่งผลลัพธ์กลับ
            return Results.Created($"/api/returnprocesses/{entity.Id}", new
            {
                Message = "ReturnProcess created successfully and vehicle pass executed",
                VehicleKey = vehicleKey,
                OrderProcess = orderProcess.ToDetailsDto()
            });
        })
        .WithName("CreateReturnProcess")
        .WithSummary("Create new ReturnProcess, call vehicle pass (from ShipmentProcess), and broadcast OrderProcess via SignalR")
        .WithDescription("สร้าง ReturnProcess, อัปเดตสถานะ OrderProcess = 'Returned', เรียก external API ผ่าน VehicleProxyService และ Broadcast แบบเรียลไทม์")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);


        // -------------------- PUT: /api/returnprocesses/{id} --------------------
        group.MapPut("/{id:int}", async (
            int id,
            ReturnProcessUpsertDto dto,
            AppDbContext db,
            IValidator<ReturnProcessUpsertDto> validator,
            OrderProcessNotifier notifier  // ✅ เพิ่ม
        ) =>
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            var entity = await db.ReturnProcesses
                .Include(rp => rp.ReturnByUser)
                .FirstOrDefaultAsync(rp => rp.Id == id);

            if (entity is null)
                return Results.NotFound();

            entity.UpdateEntity(dto);

            // ✅ โหลดและอัปเดต OrderProcess
            var orderProcess = await db.OrderProcesses
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
                .Include(op => op.ConfirmProcess)
                .Include(op => op.PreparingProcess)
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess)
                .Include(op => op.CancelledProcess)
                .Include(op => op.ReturnProcess)
                .FirstOrDefaultAsync(op => op.Id == dto.OrderProcessId);

            if (orderProcess is not null)
            {
                orderProcess.Status = "Returned";
            }

            await db.SaveChangesAsync();

            // ✅ Broadcast OrderProcess ที่อัปเดต
            if (orderProcess is not null)
            {
                await notifier.BroadcastUpdatedAsync(orderProcess.Id, orderProcess.ToDetailsDto());
            }

            var updated = await db.ReturnProcesses
                .Include(rp => rp.ReturnByUser)
                .FirstOrDefaultAsync(rp => rp.Id == id);

            return Results.Ok(updated!.ToDetailsDto());
        })
        .WithName("UpdateReturnProcess")
        .WithSummary("Update ReturnProcess and broadcast via SignalR")
        .Produces<ReturnProcessDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        // -------------------- DELETE: /api/returnprocesses/{id} --------------------
        group.MapDelete("/{id:int}", async (
            int id,
            AppDbContext db,
            OrderProcessNotifier notifier // ✅ เพิ่ม
        ) =>
        {
            var entity = await db.ReturnProcesses.FindAsync(id);
            if (entity is null)
                return Results.NotFound();

            db.ReturnProcesses.Remove(entity);

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

            if (orderProcess is not null)
            {
                orderProcess.Status = "Delivered"; // 🔙 ย้อนกลับสถานะ
                await db.SaveChangesAsync();

                // ✅ Broadcast ผ่าน SignalR
                await notifier.BroadcastUpdatedAsync(orderProcess.Id, orderProcess.ToDetailsDto());
            }

            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteReturnProcess")
        .WithSummary("Delete ReturnProcess and broadcast OrderProcess update via SignalR")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
