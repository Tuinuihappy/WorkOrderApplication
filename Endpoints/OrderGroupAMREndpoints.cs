using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Mappings;
using WorkOrderApplication.API.Services;
using WorkOrderApplication.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace WorkOrderApplication.API.Endpoints;

public static class OrderGroupAMREndpoints
{
    public static RouteGroupBuilder MapOrderGroupAMREndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET: /api/orderGroupAMR --------------------
        group.MapGet("/", async (AppDbContext db) =>
        {
            var list = await db.OrderGroupAMRs
                .AsNoTracking()
                .Select(x => x.ToListDto())
                .ToListAsync();

            return Results.Ok(list);
        })
        .WithName("GetOrderGroupAMRs")
        .WithSummary("Get all OrderGroupAMR records")
        .Produces<List<OrderGroupAMRListDto>>(StatusCodes.Status200OK)
        .WithOpenApi();

        // -------------------- GET: /api/orderGroupAMR/{id} --------------------
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.OrderGroupAMRs.FindAsync(id);

            return entity is null
                ? Results.NotFound(new { error = $"OrderGroupAMR {id} not found" })
                : Results.Ok(entity.ToDetailsDto());
        })
        .WithName("GetOrderGroupAMRById")
        .WithSummary("Get OrderGroupAMR by Id")
        .Produces<OrderGroupAMRDetailsDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi();

        // -------------------- POST: /api/orderGroupAMR --------------------
        group.MapPost("/", async (OrderGroupAMRUpsertDto dto, AppDbContext db) =>
        {
            // กัน duplicate Source+Destination
            var exists = await db.OrderGroupAMRs
                .AnyAsync(x => x.SourceStation == dto.SourceStation && x.DestinationStation == dto.DestinationStation);

            if (exists)
            {
                return Results.Conflict(new { error = "This Source-Destination pair already exists." });
            }

            var entity = dto.ToEntity();
            db.OrderGroupAMRs.Add(entity);
            await db.SaveChangesAsync();

            return Results.Created($"/api/orderGroupAMR/{entity.Id}", entity.ToDetailsDto());
        })
        .WithName("CreateOrderGroupAMR")
        .WithSummary("Create new OrderGroupAMR")
        .Produces<OrderGroupAMRDetailsDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithOpenApi();


        // -------------------- PUT: /api/orderGroupAMR/{id} --------------------------------------------------
        group.MapPut("/{id:int}", async (int id, OrderGroupAMRUpsertDto dto, AppDbContext db) =>
        {
            var entity = await db.OrderGroupAMRs.FindAsync(id);
            if (entity is null)
            {
                return Results.NotFound(new { error = $"OrderGroupAMR {id} not found" });
            }

            // กัน duplicate ถ้าแก้ไขไปซ้ำกับ record อื่น
            var exists = await db.OrderGroupAMRs
                .AnyAsync(x => x.Id != id && x.SourceStation == dto.SourceStation && x.DestinationStation == dto.DestinationStation);

            if (exists)
            {
                return Results.Conflict(new { error = "This Source-Destination pair already exists." });
            }

            entity.UpdateEntity(dto);
            await db.SaveChangesAsync();

            return Results.Ok(entity.ToDetailsDto());
        })
        .WithName("UpdateOrderGroupAMR")
        .WithSummary("Update existing OrderGroupAMR")
        .Produces<OrderGroupAMRDetailsDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithOpenApi();

        // -------------------- DELETE: /api/orderGroupAMR/{id} --------------------
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.OrderGroupAMRs.FindAsync(id);
            if (entity is null)
            {
                return Results.NotFound(new { error = $"OrderGroupAMR {id} not found" });
            }

            db.OrderGroupAMRs.Remove(entity);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteOrderGroupAMR")
        .WithSummary("Delete OrderGroupAMR by Id")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi();
        
        return group;
    }
}
