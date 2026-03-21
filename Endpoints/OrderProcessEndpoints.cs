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
            int? page = null,
            int? pageSize = null, // null = ดึงข้อมูลทั้งหมด
            string? search = null, // ✅ Global search
            string? orderNumber = null,
            string? status = null,
            string? destinationStation = null,
            string? sourceStation = null,
            string? workOrder = null,
            string? defaultLine = null,
            string? executeVehicleName = null,
            DateTime? startDate = null,
            DateTime? endDate = null
        ) =>
        {
            var currentPage = (page ?? 1) < 1 ? 1 : (page ?? 1);

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

            // ✅ Apply Global Search (Multi-field filter)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                int? searchId = int.TryParse(search, out var idVal) ? idVal : null;

                query = query.Where(op =>
                    (searchId.HasValue && op.Id == searchId.Value) || // ✅ Add ID check
                    op.OrderNumber.ToLower().Contains(lowerSearch) ||
                    op.Status.ToLower().Contains(lowerSearch) ||
                    (op.DestinationStation != null && op.DestinationStation.ToLower().Contains(lowerSearch)) || // ✅ Add OrderProcess DestinationStation
                    op.WorkOrder.Order.ToLower().Contains(lowerSearch) ||
                    op.CreatedBy.UserName.ToLower().Contains(lowerSearch) ||
                    (op.WorkOrder.OrderType != null && op.WorkOrder.OrderType.ToLower().Contains(lowerSearch)) ||
                    (op.WorkOrder.DefaultLine != null && op.WorkOrder.DefaultLine.ToLower().Contains(lowerSearch)) || // ✅ Add DefaultLine check
                    (op.ShipmentProcess != null && op.ShipmentProcess.SourceStation != null && op.ShipmentProcess.SourceStation.ToLower().Contains(lowerSearch)) ||
                    (op.ShipmentProcess != null && op.ShipmentProcess.DestinationStation != null && op.ShipmentProcess.DestinationStation.ToLower().Contains(lowerSearch)) ||
                    (op.ShipmentProcess != null && op.ShipmentProcess.ExecuteVehicleName != null && op.ShipmentProcess.ExecuteVehicleName.ToLower().Contains(lowerSearch))
                );
            }

            // ✅ Apply Specific Field Searches
            if (!string.IsNullOrWhiteSpace(orderNumber))
                query = query.Where(op => op.OrderNumber.ToLower().Contains(orderNumber.ToLower()));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(op => op.Status.ToLower().Contains(status.ToLower()));

            if (!string.IsNullOrWhiteSpace(destinationStation))
                query = query.Where(op => 
                    (op.DestinationStation != null && op.DestinationStation.ToLower().Contains(destinationStation.ToLower())) ||
                    (op.ShipmentProcess != null && op.ShipmentProcess.DestinationStation != null && op.ShipmentProcess.DestinationStation.ToLower().Contains(destinationStation.ToLower()))
                );

            if (!string.IsNullOrWhiteSpace(sourceStation))
                query = query.Where(op => 
                    op.ShipmentProcess != null && op.ShipmentProcess.SourceStation != null && op.ShipmentProcess.SourceStation.ToLower().Contains(sourceStation.ToLower())
                );

            if (!string.IsNullOrWhiteSpace(workOrder))
                query = query.Where(op => op.WorkOrder.Order.ToLower().Contains(workOrder.ToLower()));

            if (!string.IsNullOrWhiteSpace(defaultLine))
                query = query.Where(op => op.WorkOrder.DefaultLine != null && op.WorkOrder.DefaultLine.ToLower().Contains(defaultLine.ToLower()));

            if (!string.IsNullOrWhiteSpace(executeVehicleName))
                query = query.Where(op => op.ShipmentProcess != null && op.ShipmentProcess.ExecuteVehicleName != null && op.ShipmentProcess.ExecuteVehicleName.ToLower().Contains(executeVehicleName.ToLower()));

            if (startDate.HasValue)
            {
                // แปลงเวลาให้เป็น UTC ตามโซนเวลาของไทย (+7) เพื่อเปรียบเทียบในฐานข้อมูลได้อย่างถูกต้อง
                var startUtc = new DateTimeOffset(startDate.Value.Date, TimeSpan.FromHours(7)).UtcDateTime;
                query = query.Where(op => op.CreatedDate >= startUtc);
            }

            if (endDate.HasValue)
            {
                // ครอบคลุมจนถึงสิ้นวันของ endDate ก่อนที่จะแปลงเป็น UTC
                var endUtc = new DateTimeOffset(endDate.Value.Date, TimeSpan.FromHours(7)).AddDays(1).UtcDateTime;
                query = query.Where(op => op.CreatedDate < endUtc);
            }


            // ✅ Calculate Status Counts (From ALL orders - Ignore filters)
            //    Ensure all key statuses are present with 0 count if missing
            var allStatuses = new[] 
            { 
                "Order Placed", "Preparing", "In Transit", "Awaiting Pickup", 
                "Delivered", "Returned", "Cancelled" 
            };

            var dbCounts = await db.OrderProcesses
                .AsNoTracking()
                .GroupBy(op => op.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status ?? "Unknown", x => x.Count);
            
            // Merge with 0 for missing statuses
            var statusCounts = allStatuses.ToDictionary(
                status => status,
                status => dbCounts.ContainsKey(status) ? dbCounts[status] : 0
            );

            // Add any other statuses found in DB (e.g. legacy or other flows)
            foreach (var kvp in dbCounts)
            {
                if (!statusCounts.ContainsKey(kvp.Key))
                {
                    statusCounts[kvp.Key] = kvp.Value;
                }
            }

            // ✅ Count orders created today (ICT = UTC+7)
            var ictNow = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
            var todayStart = new DateTimeOffset(ictNow.Date, TimeSpan.FromHours(7)).UtcDateTime;
            var todayEnd = todayStart.AddDays(1);

            var todayOrderCount = await db.OrderProcesses
                .AsNoTracking()
                .CountAsync(op => op.CreatedDate >= todayStart && op.CreatedDate < todayEnd);

            // Get total count after filters (Verified for pagination)
            var totalCount = await query.CountAsync();

            // ตรวจสอบว่าต้องการดึงทั้งหมดหรือไม่ (แต่เพิ่ม Hard Limit ที่ 1000)
            var getAll = !pageSize.HasValue;
            var maxLimit = 1000;
            
            var size = getAll 
                ? Math.Min(totalCount, maxLimit)
                : (pageSize!.Value < 1 ? 10 : pageSize.Value);
                
            // ถ้าดึงข้อมูล getAll และไปชน Hard limit (1000) ให้บังคับใช้ Skip/Take เพื่อตัดข้อมูลด้วย
            if (getAll && totalCount > maxLimit) {
                getAll = false; 
            }

            // Apply sorting and pagination
            query = query.OrderByDescending(op => op.CreatedDate);

            if (!getAll)
            {
                query = query.Skip((currentPage - 1) * size).Take(size);
            }

            var orderProcesses = await query.ToListAsync();

            // Calculate pagination metadata
            var totalPages = size == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)size);

            var response = new
            {
                Data = orderProcesses.Select(op => op.ToListDto()),
                Pagination = new
                {
                    CurrentPage = getAll ? 1 : currentPage,
                    PageSize = getAll ? totalCount : size,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPrevious = currentPage > 1,
                    HasNext = currentPage < totalPages
                },
                Filters = new
                {
                    Search = search
                },
                StatusCounts = statusCounts, // ✅ Add Status Counts to response
                TodayOrderCount = todayOrderCount // ✅ จำนวน Order ทั้งหมดของวันนี้
            };

            return Results.Ok(response);
        });

        // -------------------- GET /api/orderprocesses/today --------------------
        group.MapGet("/today", async (
            AppDbContext db,
            int? page = null,
            int? pageSize = null, // null = ดึงข้อมูลทั้งหมด
            string? search = null // ✅ Global search
        ) =>
        {
            var currentPage = (page ?? 1) < 1 ? 1 : (page ?? 1);

            var ictNow = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
            var todayStart = new DateTimeOffset(ictNow.Date, TimeSpan.FromHours(7)).UtcDateTime;
            var todayEnd = todayStart.AddDays(1);

            var query = db.OrderProcesses.AsNoTracking()
                .Where(op => op.CreatedDate >= todayStart && op.CreatedDate < todayEnd)
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

            // ✅ Apply Global Search (Multi-field filter)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                int? searchId = int.TryParse(search, out var idVal) ? idVal : null;

                query = query.Where(op =>
                    (searchId.HasValue && op.Id == searchId.Value) || 
                    op.OrderNumber.ToLower().Contains(lowerSearch) ||
                    op.Status.ToLower().Contains(lowerSearch) ||
                    (op.DestinationStation != null && op.DestinationStation.ToLower().Contains(lowerSearch)) || 
                    op.WorkOrder.Order.ToLower().Contains(lowerSearch) ||
                    op.CreatedBy.UserName.ToLower().Contains(lowerSearch) ||
                    (op.WorkOrder.OrderType != null && op.WorkOrder.OrderType.ToLower().Contains(lowerSearch)) ||
                    (op.WorkOrder.DefaultLine != null && op.WorkOrder.DefaultLine.ToLower().Contains(lowerSearch)) || 
                    (op.ShipmentProcess != null && op.ShipmentProcess.SourceStation != null && op.ShipmentProcess.SourceStation.ToLower().Contains(lowerSearch)) ||
                    (op.ShipmentProcess != null && op.ShipmentProcess.DestinationStation != null && op.ShipmentProcess.DestinationStation.ToLower().Contains(lowerSearch)) ||
                    (op.ShipmentProcess != null && op.ShipmentProcess.ExecuteVehicleName != null && op.ShipmentProcess.ExecuteVehicleName.ToLower().Contains(lowerSearch))
                );
            }

            var totalCount = await query.CountAsync();

            var getAll = !pageSize.HasValue;
            var maxLimit = 1000;
            
            var size = getAll 
                ? Math.Min(totalCount, maxLimit)
                : (pageSize!.Value < 1 ? 10 : pageSize.Value);
                
            if (getAll && totalCount > maxLimit) {
                getAll = false; 
            }

            query = query.OrderByDescending(op => op.CreatedDate);

            if (!getAll)
            {
                query = query.Skip((currentPage - 1) * size).Take(size);
            }

            var orderProcesses = await query.ToListAsync();

            var totalPages = size == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)size);

            var response = new
            {
                Data = orderProcesses.Select(op => op.ToListDto()),
                Pagination = new
                {
                    CurrentPage = getAll ? 1 : currentPage,
                    PageSize = getAll ? totalCount : size,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPrevious = currentPage > 1,
                    HasNext = currentPage < totalPages
                },
                Filters = new
                {
                    Search = search
                }
            };

            return Results.Ok(response);
        });
        // -------------------- GET /api/orderprocesses/{id} --------------------
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var orderProcess = await db.OrderProcesses.AsNoTracking()
                .AsSplitQuery()
                .Include(op => op.CreatedBy)
                .Include(op => op.WorkOrder)
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
