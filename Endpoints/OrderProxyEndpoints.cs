using Microsoft.OpenApi.Models;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Services;
using System.Text.Json;

namespace WorkOrderApplication.API.Endpoints;

public static class OrderProxyEndpoints
{
    public static RouteGroupBuilder MapOrderProxyEndpoints(this RouteGroupBuilder group)
    {
        // -------------------- POST: /api/proxy/orderGroup --------------------
        group.MapPost("/", async (OrderGroupRequestDto dto, OrderProxyService service) =>
        {
            var result = await service.AddOrderGroupAsync(dto);

            // ✅ แปลง string → JSON object แล้วส่งกลับ
            var json = JsonSerializer.Deserialize<object>(result);

            return Results.Json(json);
        })
        .WithName("AddOrderGroupProxy")
        .WithSummary("Forward orderGroupId to external Order API")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithOpenApi();

        
        return group;
    }
}
