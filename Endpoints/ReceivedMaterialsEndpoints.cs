using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.Results;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Mappings;

namespace WorkOrderApplication.API.Endpoints;

public static class ReceivedMaterialEndpoints
{
    public static RouteGroupBuilder MapReceivedMaterialEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET ALL --------------------
        group.MapGet("/", async (AppDbContext db) =>
        {
            var materials = await db.ReceivedMaterials
                .Include(rm => rm.Material)
                .ToListAsync();

            return Results.Ok(materials.Select(rm => rm.ToListDto()));
        })
        .WithName("GetAllReceivedMaterials")
        .WithSummary("Get all received materials")
        .WithDescription("ดึงข้อมูล ReceivedMaterials ทั้งหมด")
        .Produces<List<ReceivedMaterialListDto>>(StatusCodes.Status200OK);

        // -------------------- GET BY ID --------------------
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var material = await db.ReceivedMaterials
                .Include(rm => rm.Material)
                .FirstOrDefaultAsync(rm => rm.Id == id);

            return material is null
                ? Results.NotFound()
                : Results.Ok(material.ToDetailsDto());
        })
        .WithName("GetReceivedMaterialById")
        .WithSummary("Get received material by Id")
        .WithDescription("ดึงข้อมูล ReceivedMaterial ตาม Id")
        .Produces<ReceivedMaterialDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // -------------------- POST --------------------
        group.MapPost("/{receivedProcessId:int}", async (
            int receivedProcessId,
            ReceivedMaterialUpsertDto dto,
            AppDbContext db,
            IValidator<ReceivedMaterialUpsertDto> validator) =>
        {
            ValidationResult validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            var entity = dto.ToEntity(receivedProcessId);
            db.ReceivedMaterials.Add(entity);
            await db.SaveChangesAsync();

            var created = await db.ReceivedMaterials
                .Include(rm => rm.Material)
                .FirstOrDefaultAsync(rm => rm.Id == entity.Id);

            return Results.Created($"/api/receivedmaterials/{entity.Id}", created!.ToDetailsDto());
        })
        .WithName("CreateReceivedMaterial")
        .WithSummary("Create new received material")
        .WithDescription("สร้าง ReceivedMaterial ใหม่สำหรับ ReceivedProcess")
        .Produces<ReceivedMaterialDetailsDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // -------------------- PUT --------------------
        group.MapPut("/{id:int}", async (
            int id,
            ReceivedMaterialUpsertDto dto,
            AppDbContext db,
            IValidator<ReceivedMaterialUpsertDto> validator) =>
        {
            ValidationResult validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors);

            var entity = await db.ReceivedMaterials
                .Include(rm => rm.Material)
                .FirstOrDefaultAsync(rm => rm.Id == id);

            if (entity is null) return Results.NotFound();

            entity.UpdateEntity(dto);
            await db.SaveChangesAsync();

            return Results.Ok(entity.ToDetailsDto());
        })
        .WithName("UpdateReceivedMaterial")
        .WithSummary("Update received material")
        .WithDescription("แก้ไขข้อมูล ReceivedMaterial ตาม Id")
        .Produces<ReceivedMaterialDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // -------------------- DELETE --------------------
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.ReceivedMaterials.FindAsync(id);
            if (entity is null) return Results.NotFound();

            db.ReceivedMaterials.Remove(entity);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteReceivedMaterial")
        .WithSummary("Delete received material")
        .WithDescription("ลบ ReceivedMaterial ตาม Id")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
