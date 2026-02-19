using Microsoft.EntityFrameworkCore;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Mappings;
using WorkOrderApplication.API.Validators;
using FluentValidation;

namespace WorkOrderApplication.API.Endpoints;

public static class WorkOrderEndpoints
{
    public static RouteGroupBuilder MapWorkOrderEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET: /api/workorders (Pagination + Filter) --------------------
        group.MapGet("/", async (
            AppDbContext db,
            int? page,
            int? pageSize,
            string? search) =>
        {
            var currentPage = page ?? 1;
            var size = pageSize ?? 10;
            if (currentPage < 1) currentPage = 1;
            if (size < 1) size = 10;
            if (size > 100) size = 100;

            // ✅ AsNoTracking: ไม่ track entity (ลด memory + เร็วขึ้น)
            IQueryable<WorkOrder> query = db.WorkOrders.AsNoTracking();

            // ✅ Search ค้นหาจาก Order, OrderType, Plant, Material พร้อมกัน
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(w =>
                    w.Order.Contains(term) ||
                    w.OrderType.Contains(term) ||
                    w.Plant.Contains(term) ||
                    w.Material.Contains(term));
            }

            // ✅ Count (sequential - DbContext ไม่ thread-safe)
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)size);

            // ✅ Pagination + Inline Projection (ดึงเฉพาะ column ที่ใช้)
            var workOrders = await query
                .OrderByDescending(w => w.CreatedDate)
                .Skip((currentPage - 1) * size)
                .Take(size)
                .Select(w => new WorkOrderListDto(
                    w.Id,
                    w.Order,
                    w.OrderType,
                    w.Plant,
                    w.Material,
                    w.Quantity,
                    w.Unit,
                    w.BasicFinishDate,
                    w.DefaultLine,
                    w.CreatedDate
                ))
                .ToListAsync();

            return Results.Ok(new
            {
                totalCount,
                totalPages,
                currentPage,
                pageSize = size,
                data = workOrders
            });
        })
        .WithName("GetWorkOrders")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

        // -------------------- GET: /api/workorders/{id} --------------------
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var workOrder = await db.WorkOrders
                .AsNoTracking()          // ✅ ไม่ track entity
                .AsSplitQuery()          // ✅ แยก query ป้องกัน cartesian explosion
                .Include(w => w.Materials)
                .Include(w => w.OrderProcesses)
                .FirstOrDefaultAsync(w => w.Id == id);

            return workOrder is null
                ? Results.NotFound()
                : Results.Ok(workOrder.ToDetailsDto());
        })
        .WithName("GetWorkOrderById")
        .Produces<WorkOrderDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

        // -------------------- POST: /api/workorders --------------------
        group.MapPost("/", async (
            WorkOrderCreateDto dto,
            AppDbContext db,
            MesTdcClient mes,
            IValidator<WorkOrderCreateDto> validator) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Results.BadRequest(validation.Errors);

            var workOrder = dto.ToEntity();

            // ✅ ดึง DefaultLine จาก MES ทันทีเมื่อสร้าง WorkOrder
            try
            {
                var routingData = $"0}}{dto.Order}";
                var raw = await mes.CallAsync(testType: "GET_MO_INFO", routingData: routingData);

                if (raw.TryGetProperty("description", out var desc) &&
                    desc.ValueKind == System.Text.Json.JsonValueKind.Object &&
                    desc.TryGetProperty("Default Line", out var lineProp) &&
                    lineProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    workOrder.DefaultLine = lineProp.GetString();
                }
            }
            catch
            {
                // ⚠️ ถ้า MES ติดต่อไม่ได้ ไม่ block การสร้าง WorkOrder
                // Background Service จะ sync ให้ภายหลัง
            }

            db.WorkOrders.Add(workOrder);
            await db.SaveChangesAsync();

            return Results.Created($"/api/workorders/{workOrder.Id}", workOrder.ToDetailsDto());
        })
        .WithName("CreateWorkOrder")
        .Produces<WorkOrderDetailsDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

    // -------------------- PUT: /api/workorders/{id} --------------------
    group.MapPut("/{id:int}", async (
        int id,
        WorkOrderUpdateDto dto,
        AppDbContext db,
        IValidator<WorkOrderUpdateDto> validator) =>
    {
        // 1. Validate
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Results.BadRequest(validation.Errors);

        // 2. Find WorkOrder (รวม Materials)
        var workOrder = await db.WorkOrders
            .Include(w => w.Materials)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workOrder is null)
            return Results.NotFound();

        // 3. Update Entity จาก DTO
        workOrder.UpdateFromDto(dto);

        await db.SaveChangesAsync();

        // 4. โหลดใหม่พร้อม Navigation Properties
        var updated = await db.WorkOrders
            .Include(w => w.Materials)
            .Include(w => w.OrderProcesses)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (updated is null)
            return Results.NotFound();

        // 5. Mapping → Dto
        return Results.Ok(updated.ToDetailsDto());
    })
    .WithName("UpdateWorkOrder")
    .Produces<WorkOrderDetailsDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status500InternalServerError)
    .WithOpenApi();


        // -------------------- DELETE: /api/workorders/{id} --------------------
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var workOrder = await db.WorkOrders
                .Include(w => w.Materials)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workOrder is null)
                return Results.NotFound();

            db.WorkOrders.Remove(workOrder);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteWorkOrder")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

        return group;
    }
}
