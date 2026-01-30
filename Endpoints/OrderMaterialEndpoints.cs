using Microsoft.EntityFrameworkCore;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Mappings;
using WorkOrderApplication.API.Validators;
using FluentValidation;

namespace WorkOrderApplication.API.Endpoints;

public static class OrderMaterialEndpoints
{
    public static RouteGroupBuilder MapOrderMaterialEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET: /api/ordermaterials --------------------
        group.MapGet("/", async (AppDbContext db) =>
        {
            var items = await db.OrderMaterials
                .Include(om => om.Material)
                .Select(om => om.ToListDto())
                .ToListAsync();

            return Results.Ok(items);
        })
        .WithName("GetOrderMaterials")
        .Produces<List<OrderMaterialListDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

        // -------------------- GET: /api/ordermaterials/{id} --------------------
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.OrderMaterials
                .Include(om => om.Material)
                .FirstOrDefaultAsync(om => om.Id == id);

            return entity is null
                ? Results.NotFound()
                : Results.Ok(entity.ToDetailsDto());
        })
        .WithName("GetOrderMaterialById")
        .Produces<OrderMaterialDetailsDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi();

        // -------------------- POST: /api/ordermaterials --------------------
        group.MapPost("/", async (OrderMaterialUpsertDto dto, AppDbContext db, IValidator<OrderMaterialUpsertDto> validator) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var entity = dto.ToEntity();
            db.OrderMaterials.Add(entity);
            await db.SaveChangesAsync();

            return Results.Created($"/api/ordermaterials/{entity.Id}", entity.ToDetailsDto());
        })
        .WithName("CreateOrderMaterial")
        .Produces<OrderMaterialDetailsDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithOpenApi();

        // -------------------- PUT: /api/ordermaterials/{id} --------------------
        group.MapPut("/{id:int}", async (int id, OrderMaterialUpsertDto dto, AppDbContext db, IValidator<OrderMaterialUpsertDto> validator) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var entity = await db.OrderMaterials.FindAsync(id);
            if (entity is null)
                return Results.NotFound();

            entity.UpdateEntity(dto);
            await db.SaveChangesAsync();

            return Results.Ok(entity.ToDetailsDto());
        })
        .WithName("UpdateOrderMaterial")
        .Produces<OrderMaterialDetailsDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithOpenApi();

        // -------------------- DELETE: /api/ordermaterials/{id} --------------------
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.OrderMaterials.FindAsync(id);
            if (entity is null)
                return Results.NotFound();

            db.OrderMaterials.Remove(entity);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteOrderMaterial")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi();

        return group;
    }
}
