using Microsoft.EntityFrameworkCore;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Mappings;

namespace WorkOrderApplication.API.Endpoints;

public static class MaterialEndpoints
{
    public static RouteGroupBuilder MapMaterialEndpoints(this RouteGroupBuilder group)
    {
        var materials = group.MapGroup("/materials")
            .WithTags("Materials");

        // -------------------- GET: All --------------------
        materials.MapGet("/", async (AppDbContext db) =>
        {
            var items = await db.Materials
                .Include(m => m.WorkOrder)
                .Select(m => m.ToListDto())
                .ToListAsync();

            return Results.Ok(items);
        });

        // -------------------- GET: By Id --------------------
        materials.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Materials
                .Include(m => m.WorkOrder)
                .FirstOrDefaultAsync(m => m.Id == id);

            return entity is null
                ? Results.NotFound()
                : Results.Ok(entity.ToDetailsDto());
        });

        // -------------------- POST: Create (ต้องระบุ WorkOrderId จาก route) --------------------
        materials.MapPost("/workorders/{workOrderId:int}", async (int workOrderId, MaterialCreateDto dto, AppDbContext db) =>
        {
            var workOrder = await db.WorkOrders.FindAsync(workOrderId);
            if (workOrder is null)
                return Results.NotFound($"WorkOrder {workOrderId} not found");

            var entity = dto.ToEntity(workOrderId);
            entity.WorkOrderId = workOrderId; // ✅ กำหนดเอง ไม่ให้ client ใส่มา

            db.Materials.Add(entity);
            await db.SaveChangesAsync();

            return Results.Created($"/api/materials/{entity.Id}", entity.ToDetailsDto());
        });

        // -------------------- PUT: Update --------------------
        materials.MapPut("/{id:int}", async (int id, MaterialUpdateDto dto, AppDbContext db) =>
        {
            var entity = await db.Materials.FindAsync(id);
            if (entity is null)
                return Results.NotFound();

            entity.UpdateEntity(dto);
            await db.SaveChangesAsync();

            return Results.Ok(entity.ToDetailsDto());
        });

        // -------------------- DELETE: Remove --------------------
        materials.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Materials.FindAsync(id);
            if (entity is null)
                return Results.NotFound();

            db.Materials.Remove(entity);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return materials;
    }
}
