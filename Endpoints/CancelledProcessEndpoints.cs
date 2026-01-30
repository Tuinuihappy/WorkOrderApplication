// using Microsoft.EntityFrameworkCore;
// using FluentValidation;
// using FluentValidation.Results;
// using WorkOrderApplication.API.Data;
// using WorkOrderApplication.API.Dtos;
// using WorkOrderApplication.API.Entities;
// using WorkOrderApplication.API.Mappings;

// namespace WorkOrderApplication.API.Endpoints;

// public static class CancelledProcessEndpoints
// {
//     public static RouteGroupBuilder MapCancelledProcessEndpoints(this RouteGroupBuilder group)
//     {
//         // -------------------- GET ALL --------------------
//         group.MapGet("/", async (AppDbContext db) =>
//         {
//             var items = await db.CancelledProcesses
//                 .Include(cp => cp.CancelledBy)
//                 .ToListAsync();

//             return Results.Ok(items.Select(cp => cp.ToListDto()));
//         })
//         .WithName("GetAllCancelledProcesses")
//         .WithSummary("Get all cancelled processes")
//         .Produces<List<CancelledProcessListDto>>(StatusCodes.Status200OK);

//         // -------------------- GET BY ID --------------------
//         group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
//         {
//             var item = await db.CancelledProcesses
//                 .Include(cp => cp.CancelledBy)
//                 .FirstOrDefaultAsync(cp => cp.Id == id);

//             return item is null
//                 ? Results.NotFound()
//                 : Results.Ok(item.ToDetailsDto());
//         })
//         .WithName("GetCancelledProcessById")
//         .WithSummary("Get cancelled process by Id")
//         .Produces<CancelledProcessDetailsDto>(StatusCodes.Status200OK)
//         .Produces(StatusCodes.Status404NotFound);

//         // -------------------- POST --------------------
//         group.MapPost("/", async (
//             CancelledProcessUpsertDto dto,
//             AppDbContext db,
//             IValidator<CancelledProcessUpsertDto> validator) =>
//         {
//             // ✅ Validate input
//             ValidationResult validationResult = await validator.ValidateAsync(dto);
//             if (!validationResult.IsValid)
//                 return Results.BadRequest(validationResult.Errors);

//             // ✅ แปลง DTO → Entity
//             var entity = dto.ToEntity();
//             db.CancelledProcesses.Add(entity);

//             // ✅ หา OrderProcess ที่เกี่ยวข้อง
//             var orderProcess = await db.OrderProcesses
//                 .FirstOrDefaultAsync(op => op.Id == dto.OrderProcessId);

//             if (orderProcess is null)
//             {
//                 return Results.BadRequest(new
//                 {
//                     error = $"OrderProcess with Id {dto.OrderProcessId} not found."
//                 });
//             }

//             // ✅ อัปเดตสถานะ OrderProcess → Preparing
//             orderProcess.Status = "Cancelled";

//             // ✅ บันทึกการเปลี่ยนแปลงทั้งหมด
//             await db.SaveChangesAsync();

//             // ✅ โหลดข้อมูลพร้อม Navigation Property (เช่น CancelledBy)
//             var created = await db.CancelledProcesses
//                 .Include(cp => cp.CancelledBy)
//                 .FirstOrDefaultAsync(cp => cp.Id == entity.Id);

//             return Results.Created($"/api/cancelledprocesses/{entity.Id}", created!.ToDetailsDto());
//         })
//         .WithName("CreateCancelledProcess")
//         .WithSummary("Create new cancelled process and update related OrderProcess status to 'Preparing'")
//         .Produces<CancelledProcessDetailsDto>(StatusCodes.Status201Created)
//         .Produces(StatusCodes.Status400BadRequest);


//         // -------------------- PUT --------------------
//         group.MapPut("/{id:int}", async (
//             int id,
//             CancelledProcessUpsertDto dto,
//             AppDbContext db,
//             IValidator<CancelledProcessUpsertDto> validator) =>
//         {
//             ValidationResult validationResult = await validator.ValidateAsync(dto);
//             if (!validationResult.IsValid)
//                 return Results.BadRequest(validationResult.Errors);

//             var entity = await db.CancelledProcesses
//                 .Include(cp => cp.CancelledBy)
//                 .FirstOrDefaultAsync(cp => cp.Id == id);

//             if (entity is null) return Results.NotFound();

//             entity.UpdateEntity(dto);
//             await db.SaveChangesAsync();

//             return Results.Ok(entity.ToDetailsDto());
//         })
//         .WithName("UpdateCancelledProcess")
//         .WithSummary("Update cancelled process")
//         .Produces<CancelledProcessDetailsDto>(StatusCodes.Status200OK)
//         .Produces(StatusCodes.Status400BadRequest)
//         .Produces(StatusCodes.Status404NotFound);

//         // -------------------- DELETE --------------------
//         group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
//         {
//             var entity = await db.CancelledProcesses.FindAsync(id);
//             if (entity is null) return Results.NotFound();

//             db.CancelledProcesses.Remove(entity);
//             await db.SaveChangesAsync();

//             return Results.NoContent();
//         })
//         .WithName("DeleteCancelledProcess")
//         .WithSummary("Delete cancelled process")
//         .Produces(StatusCodes.Status204NoContent)
//         .Produces(StatusCodes.Status404NotFound);

//         return group;
//     }
// }


using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.Results;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Mappings;
using WorkOrderApplication.API.Services; // ✅ ใช้ OrderProcessNotifier

namespace WorkOrderApplication.API.Endpoints;

public static class CancelledProcessEndpoints
{
    public static RouteGroupBuilder MapCancelledProcessEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET ALL --------------------
        group.MapGet("/", async (AppDbContext db) =>
        {
            var items = await db.CancelledProcesses
                .Include(cp => cp.CancelledBy)
                .ToListAsync();

            return Results.Ok(items.Select(cp => cp.ToListDto()));
        })
        .WithName("GetAllCancelledProcesses")
        .WithSummary("Get all cancelled processes")
        .Produces<List<CancelledProcessListDto>>(StatusCodes.Status200OK);

        // -------------------- GET BY ID --------------------
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var item = await db.CancelledProcesses
                .Include(cp => cp.CancelledBy)
                .FirstOrDefaultAsync(cp => cp.Id == id);

            return item is null
                ? Results.NotFound()
                : Results.Ok(item.ToDetailsDto());
        })
        .WithName("GetCancelledProcessById")
        .WithSummary("Get cancelled process by Id")
        .Produces<CancelledProcessDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // -------------------- POST --------------------
        group.MapPost("/", async (
            CancelledProcessUpsertDto dto,
            AppDbContext db,
            IValidator<CancelledProcessUpsertDto> validator,
            OrderProcessNotifier notifier   // ✅ เพิ่ม
        ) =>
        {
            // ✅ Validate input
            ValidationResult validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            // ✅ แปลง DTO → Entity
            var entity = dto.ToEntity();
            db.CancelledProcesses.Add(entity);

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
                .FirstOrDefaultAsync(op => op.Id == dto.OrderProcessId);

            if (orderProcess is null)
            {
                return Results.BadRequest(new
                {
                    error = $"OrderProcess with Id {dto.OrderProcessId} not found."
                });
            }

            // ✅ อัปเดตสถานะ OrderProcess → Cancelled
            orderProcess.Status = "Cancelled";
            await db.SaveChangesAsync();

            // ✅ โหลดข้อมูลพร้อม Navigation Property (เช่น CancelledBy)
            var created = await db.CancelledProcesses
                .Include(cp => cp.CancelledBy)
                .FirstOrDefaultAsync(cp => cp.Id == entity.Id);

            // ✅ Broadcast OrderProcess ที่อัปเดตผ่าน SignalR
            await notifier.BroadcastUpdatedAsync(orderProcess.Id, orderProcess.ToDetailsDto());

            return Results.Created($"/api/cancelledprocesses/{entity.Id}", created!.ToDetailsDto());
        })
        .WithName("CreateCancelledProcess")
        .WithSummary("Create new cancelled process and broadcast OrderProcess via SignalR")
        .Produces<CancelledProcessDetailsDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // -------------------- PUT --------------------
        group.MapPut("/{id:int}", async (
            int id,
            CancelledProcessUpsertDto dto,
            AppDbContext db,
            IValidator<CancelledProcessUpsertDto> validator,
            OrderProcessNotifier notifier   // ✅ เพิ่ม
        ) =>
        {
            ValidationResult validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            var entity = await db.CancelledProcesses
                .Include(cp => cp.CancelledBy)
                .FirstOrDefaultAsync(cp => cp.Id == id);

            if (entity is null) return Results.NotFound();

            entity.UpdateEntity(dto);
            await db.SaveChangesAsync();

            // ✅ โหลด OrderProcess เพื่อ broadcast
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
                await notifier.BroadcastUpdatedAsync(orderProcess.Id, orderProcess.ToDetailsDto());

            return Results.Ok(entity.ToDetailsDto());
        })
        .WithName("UpdateCancelledProcess")
        .WithSummary("Update cancelled process and broadcast OrderProcess via SignalR")
        .Produces<CancelledProcessDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // -------------------- DELETE --------------------
        group.MapDelete("/{id:int}", async (
            int id,
            AppDbContext db,
            OrderProcessNotifier notifier   // ✅ เพิ่ม
        ) =>
        {
            var entity = await db.CancelledProcesses.FindAsync(id);
            if (entity is null) return Results.NotFound();

            db.CancelledProcesses.Remove(entity);

            // ✅ หา OrderProcess ที่เกี่ยวข้องเพื่อ revert สถานะ
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
                orderProcess.Status = "Pending"; // 🔙 ย้อนกลับสถานะ
                await db.SaveChangesAsync();

                // ✅ Broadcast Real-time
                await notifier.BroadcastUpdatedAsync(orderProcess.Id, orderProcess.ToDetailsDto());
            }

            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteCancelledProcess")
        .WithSummary("Delete cancelled process and broadcast OrderProcess via SignalR")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
