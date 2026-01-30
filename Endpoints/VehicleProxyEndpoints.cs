using WorkOrderApplication.API.Services;

namespace WorkOrderApplication.API.Endpoints;

public static class VehicleProxyEndpoints
{
    public static RouteGroupBuilder MapVehicleProxyEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/vehicle/pass/{vehicleKey}", async (string vehicleKey, VehicleProxyService service) =>
        {
            var result = await service.PassVehicleAsync(vehicleKey);
            if (result == null)
                return Results.Problem("Failed to call vehicle pass API");

            return Results.Ok(new
            {
                VehicleKey = vehicleKey,
                Message = "Vehicle pass executed successfully",
                Result = result
            });
        })
        .WithSummary("Call external vehicle pass API")
        .WithDescription("โพสต์ข้อมูลไปยัง external API เพื่อสั่งให้ vehicle ผ่าน (pass)")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        return group;
    }
}
