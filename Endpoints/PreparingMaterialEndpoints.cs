using Microsoft.EntityFrameworkCore;
using FluentValidation;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Mappings;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Endpoints;

public static class PreparingMaterialEndpoints
{
    public static RouteGroupBuilder MapPreparingMaterialEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET: /api/preparingmaterials --------------------
        group.MapGet("/", async (AppDbContext db) =>
        {
            var preparingMaterials = await db.PreparingMaterials
                .Include(pm => pm.Material)
                .ToListAsync();

            return Results.Ok(preparingMaterials.Select(pm => pm.ToListDto()));
        })
        .WithName("GetAllPreparingMaterials")
        .WithSummary("Get all PreparingMaterials")
        .WithDescription("ดึงข้อมูล PreparingMaterials ทั้งหมดในระบบ")
        .Produces<List<PreparingMaterialListDto>>(StatusCodes.Status200OK);

        // -------------------- GET: /api/preparingmaterials/{id} --------------------
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var preparingMaterial = await db.PreparingMaterials
                .Include(pm => pm.Material)
                .FirstOrDefaultAsync(pm => pm.Id == id);

            return preparingMaterial is null
                ? Results.NotFound()
                : Results.Ok(preparingMaterial.ToDetailsDto());
        })
        .WithName("GetPreparingMaterialById")
        .WithSummary("Get PreparingMaterial by Id")
        .Produces<PreparingMaterialDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // -------------------- POST: /api/preparingmaterials --------------------
        group.MapPost("/", async (
            PreparingMaterialUpsertDto dto,
            IValidator<PreparingMaterialUpsertDto> validator,
            AppDbContext db) =>
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            var entity = dto.ToEntity();
            db.PreparingMaterials.Add(entity);
            await db.SaveChangesAsync();

            return Results.Created($"/api/preparingmaterials/{entity.Id}", entity.ToDetailsDto());
        })
        .WithName("CreatePreparingMaterial")
        .WithSummary("Create new PreparingMaterial")
        .Produces<PreparingMaterialDetailsDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // -------------------- PUT: /api/preparingmaterials/{id} --------------------
        group.MapPut("/{id:int}", async (
            int id,
            PreparingMaterialUpsertDto dto,
            IValidator<PreparingMaterialUpsertDto> validator,
            AppDbContext db) =>
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            var entity = await db.PreparingMaterials.FindAsync(id);
            if (entity is null)
                return Results.NotFound();

            entity.UpdateEntity(dto);
            await db.SaveChangesAsync();

            return Results.Ok(entity.ToDetailsDto());
        })
        .WithName("UpdatePreparingMaterial")
        .WithSummary("Update PreparingMaterial by Id")
        .Produces<PreparingMaterialDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        // -------------------- DELETE: /api/preparingmaterials/{id} --------------------
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.PreparingMaterials.FindAsync(id);
            if (entity is null)
                return Results.NotFound();

            db.PreparingMaterials.Remove(entity);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeletePreparingMaterial")
        .WithSummary("Delete PreparingMaterial by Id")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
