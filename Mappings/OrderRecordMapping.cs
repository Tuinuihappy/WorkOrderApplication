using System.Text.Json;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
namespace WorkOrderApplication.API.Mappings;

public static class OrderRecordMapping
{
    public static OrderRecord ToOrderRecord(this OrderRecordDto order)
        => new()
        {
            Id = order.Id,
            OrderName = order.OrderName,
            LastStatus = order.OrderState.ToString(),
            ExecutingIndex = order.ExecutingIndex,
            Progress = order.Progress,
            LastUpdated = DateTime.UtcNow,
            RawResponse = JsonSerializer.Serialize(order),
            Source = "RIOT",
            UpdatedBy = "BackgroundService"
        };
}
