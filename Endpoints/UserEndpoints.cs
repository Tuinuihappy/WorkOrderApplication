using Microsoft.EntityFrameworkCore;
using FluentValidation;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Mappings;


namespace WorkOrderApplication.API.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- GET /api/users --------------------
        group.MapGet("/", async (AppDbContext db) =>
        {
            var users = await db.Users.ToListAsync();
            return Results.Ok(users.Select(u => u.ToListDto()));
        })
        .WithName("GetAllUsers")
        .WithSummary("Get all users")
        .WithDescription("ดึงข้อมูลผู้ใช้งานทั้งหมด")
        .Produces<List<UserListDto>>(StatusCodes.Status200OK);

        // -------------------- GET /api/users/{id} --------------------
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null) return Results.NotFound();

            return Results.Ok(user.ToDetailsDto());
        })
        .WithName("GetUserById")
        .WithSummary("Get user by ID")
        .WithDescription("ดึงข้อมูลผู้ใช้งานตามรหัส")
        .Produces<UserDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // -------------------- POST /api/users --------------------
        group.MapPost("/", async (UserUpsertDto dto, AppDbContext db, IValidator<UserUpsertDto> validator) =>
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors.Select(e => new
                {
                    propertyName = e.PropertyName,
                    errorMessage = e.ErrorMessage
                }));
            }

            var user = dto.ToEntity();
            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Results.Created($"/api/users/{user.Id}", user.ToDetailsDto());
        })
        .WithName("CreateUser")
        .WithSummary("Create a new user")
        .WithDescription("สร้างผู้ใช้งานใหม่")
        .Produces<UserDetailsDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // -------------------- PUT /api/users/{id} --------------------
        group.MapPut("/{id:int}", async (int id, UserUpsertDto dto, AppDbContext db, IValidator<UserUpsertDto> validator) =>
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors.Select(e => new
                {
                    propertyName = e.PropertyName,
                    errorMessage = e.ErrorMessage
                }));
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null) return Results.NotFound();

            user.UpdateEntity(dto);
            await db.SaveChangesAsync();

            return Results.Ok(user.ToDetailsDto());
        })
        .WithName("UpdateUser")
        .WithSummary("Update user by ID")
        .WithDescription("แก้ไขข้อมูลผู้ใช้งาน")
        .Produces<UserDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // -------------------- DELETE /api/users/{id} --------------------
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null) return Results.NotFound();

            db.Users.Remove(user);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteUser")
        .WithSummary("Delete user by ID")
        .WithDescription("ลบผู้ใช้งานออกจากระบบ")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
