using System.Text.Json;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace WorkOrderApplication.API.Services;

public class WorkOrderLineSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkOrderLineSyncService> _logger;

    public WorkOrderLineSyncService(
        IServiceScopeFactory scopeFactory,
        ILogger<WorkOrderLineSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[WorkOrderLineSyncService] 🚀 Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncDefaultLinesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WorkOrderLineSyncService] ❌ Error during sync");
            }

            // Wait 1 minute
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task SyncDefaultLinesAsync(CancellationToken token)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mesClient = scope.ServiceProvider.GetRequiredService<MesTdcClient>();

        // ✅ ดึง WorkOrder ทั้งหมด
        var workOrders = await db.WorkOrders
            .OrderByDescending(w => w.CreatedDate)
            .ToListAsync(token);

        if (!workOrders.Any()) return;

        _logger.LogInformation("[WorkOrderLineSyncService] 🔄 Syncing {Count} orders", workOrders.Count);

        int updatedCount = 0;

        foreach (var wo in workOrders)
        {
            if (token.IsCancellationRequested) break;

            try
            {
                var routingData = $"0}}{wo.Order}";

                var raw = await mesClient.CallAsync(
                    testType: "GET_MO_INFO",
                    routingData: routingData
                );

                // ✅ ดึงค่า DefaultLine ใหม่จาก MES
                var newDefaultLine = ExtractDefaultLine(raw);

                // ✅ อัพเดทเฉพาะเมื่อค่าเปลี่ยนแปลง
                if (newDefaultLine is not null && newDefaultLine != wo.DefaultLine)
                {
                    _logger.LogInformation(
                        "[WorkOrderLineSyncService] 📝 Order {Order}: DefaultLine changed '{Old}' → '{New}'",
                        wo.Order, wo.DefaultLine ?? "(null)", newDefaultLine);

                    wo.DefaultLine = newDefaultLine;
                    wo.UpdatedDate = DateTime.UtcNow;
                    updatedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[WorkOrderLineSyncService] ⚠️ Failed for Order {Order}", wo.Order);
            }

            // Small delay to be nice to MES
            await Task.Delay(100, token);
        }

        if (updatedCount > 0)
        {
            await db.SaveChangesAsync(token);
            _logger.LogInformation("[WorkOrderLineSyncService] ✅ Updated {Count} orders", updatedCount);
        }
        else
        {
            _logger.LogInformation("[WorkOrderLineSyncService] ✅ No changes detected");
        }
    }

    /// <summary>
    /// ดึงค่า "Default Line" จาก MES response
    /// </summary>
    private static string? ExtractDefaultLine(JsonElement raw)
    {
        if (!raw.TryGetProperty("description", out var desc))
            return null;

        // กรณี description เป็น JSON Object (เช่น { "Default Line": "STF03", ... })
        if (desc.ValueKind == JsonValueKind.Object)
        {
            if (desc.TryGetProperty("Default Line", out var lineProp) &&
                lineProp.ValueKind == JsonValueKind.String)
            {
                return lineProp.GetString();
            }
        }

        // กรณี description เป็น String
        if (desc.ValueKind == JsonValueKind.String)
        {
            var text = desc.GetString();
            if (!string.IsNullOrWhiteSpace(text) && text != "{0}")
                return text;
        }

        return null;
    }
}
