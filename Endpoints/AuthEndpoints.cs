using Microsoft.EntityFrameworkCore;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Mappings;
using WorkOrderApplication.API.Services;

namespace WorkOrderApplication.API.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/login", async (LoginRequestDto request, AppDbContext db, IAuthService authService) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.EmployeeId == request.EmployeeId);
            
            if (user == null || !authService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Results.BadRequest(new { message = "Invalid EmployeeId or Password / รหัสพนักงานหรือรหัสผ่านไม่ถูกต้อง" });
            }

            var token = authService.GenerateJwtToken(user);

            return Results.Ok(new LoginResponseDto(token, user.ToDetailsDto()));
        })
        .WithName("Login")
        .WithSummary("User login to get JWT token")
        .WithDescription("เข้าสู่ระบบเพื่อรับ Token")
        .Produces<LoginResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        return group;
    }
}
