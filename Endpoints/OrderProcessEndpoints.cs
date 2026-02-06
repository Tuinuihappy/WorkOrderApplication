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
        group.MapGet("/", async (
            AppDbContext db,
            int page = 1,
            int pageSize = 10,
            int? search = null,
            string? status = null,
            string? fromDate = null,
            string? toDate = null,
            string? workOrderNumber = null,
            string? createdByName = null,
            string? lineName = null,
            string? sourceStation = null,
            string? destinationStation = null,
            string? executeVehicleName = null
        ) =>
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Max page size limit

            // Parse date parameters
            DateTime? fromDateParsed = null;
            DateTime? toDateParsed = null;

            if (!string.IsNullOrWhiteSpace(fromDate))
            {
                if (DateTime.TryParse(fromDate, out var parsedFrom))
                {
                    // Specify UTC kind for PostgreSQL compatibility
                    fromDateParsed = DateTime.SpecifyKind(parsedFrom, DateTimeKind.Utc);
                }
            }

            if (!string.IsNullOrWhiteSpace(toDate))
            {
                if (DateTime.TryParse(toDate, out var parsedTo))
                {
                    // Specify UTC kind for PostgreSQL compatibility
                    toDateParsed = DateTime.SpecifyKind(parsedTo, DateTimeKind.Utc);
                }
            }

            // Build query with filters
            var query = db.OrderProcesses.AsNoTracking()
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
                .Include(op => op.ConfirmProcess)
                .Include(op => op.PreparingProcess)
                .Include(op => op.ShipmentProcess)
                .Include(op => op.ReceiveProcess)
                .Include(op => op.CancelledProcess)
                .Include(op => op.ReturnProcess)
                .AsSplitQuery()
                .AsQueryable();

            // Apply search filter (OrderProcess Id)
            if (search.HasValue)
            {
                query = query.Where(op => op.Id == search.Value);
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(op => op.Status == status);
            }

            // Apply date range filters
            if (fromDateParsed.HasValue)
            {
                query = query.Where(op => op.CreatedDate >= fromDateParsed.Value);
            }

            if (toDateParsed.HasValue)
            {
                // Include the entire day by setting time to end of day
                var endOfDay = toDateParsed.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(op => op.CreatedDate <= endOfDay);
            }

            // Apply workOrderNumber filter (via WorkOrder navigation property)
            if (!string.IsNullOrWhiteSpace(workOrderNumber))
            {
                query = query.Where(op => op.WorkOrder.WorkOrderNumber.Contains(workOrderNumber));
            }

            // Apply createdByName filter (via CreatedBy navigation property)
            if (!string.IsNullOrWhiteSpace(createdByName))
            {
                query = query.Where(op => op.CreatedBy.UserName.Contains(createdByName));
            }

            // Apply lineName filter (via WorkOrder navigation property)
            if (!string.IsNullOrWhiteSpace(lineName))
            {
                query = query.Where(op => op.WorkOrder.LineName.Contains(lineName));
            }

            // Apply sourceStation filter (via ShipmentProcess navigation property)
            if (!string.IsNullOrWhiteSpace(sourceStation))
            {
                query = query.Where(op => op.ShipmentProcess != null && op.ShipmentProcess.SourceStation == sourceStation);
            }

            // Apply destinationStation filter (via ShipmentProcess navigation property)
            if (!string.IsNullOrWhiteSpace(destinationStation))
            {
                query = query.Where(op => op.ShipmentProcess != null && op.ShipmentProcess.DestinationStation == destinationStation);
            }

            // Apply executeVehicleName filter (via ShipmentProcess navigation property)
            if (!string.IsNullOrWhiteSpace(executeVehicleName))
            {
                query = query.Where(op => op.ShipmentProcess != null && op.ShipmentProcess.ExecuteVehicleName != null && op.ShipmentProcess.ExecuteVehicleName.Contains(executeVehicleName));
            }

            // Get total count after filters
            var totalCount = await query.CountAsync();

            // Apply sorting and pagination
            var orderProcesses = await query
                .OrderByDescending(op => op.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calculate pagination metadata
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var response = new
            {
                Data = orderProcesses.Select(op => op.ToListDto()),
                Pagination = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPrevious = page > 1,
                    HasNext = page < totalPages
                },
                Filters = new
                {
                    Search = search,
                    Status = status,
                    FromDate = fromDate,
                    ToDate = toDate,
                    WorkOrderNumber = workOrderNumber,
                    CreatedByName = createdByName,
                    LineName = lineName,
                    SourceStation = sourceStation,
                    DestinationStation = destinationStation,
                    ExecuteVehicleName = executeVehicleName
                }
            };

            return Results.Ok(response);
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
                .AsSplitQuery()
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
