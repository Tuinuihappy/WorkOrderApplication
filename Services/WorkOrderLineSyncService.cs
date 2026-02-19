using System.Text.Json;
using WorkOrderApplication.API.Data;
using Microsoft.EntityFrameworkCore;

namespace WorkOrderApplication.API.Services;

public class WorkOrderLineSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkOrderLineSyncService> _logger;

    // ✅ Concurrency limit สำหรับ MES calls (ป้องกัน overload)
    private const int MaxConcurrency = 5;

    public WorkOrderLineSyncService(
        IServiceScopeFactory scopeFactory,
        ILogger<WorkOrderLineSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[WorkOrderLineSyncService] 🚀 Service started (MaxConcurrency={Max})", MaxConcurrency);

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

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    /// <summary>
    /// Lightweight DTO สำหรับ projection (ไม่ track entity)
    /// </summary>
    private record WorkOrderSlim(int Id, string Order, string? DefaultLine);

    private async Task SyncDefaultLinesAsync(CancellationToken token)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mesClient = scope.ServiceProvider.GetRequiredService<MesTdcClient>();

        // ✅ 1. Projection: ดึงเฉพาะ field ที่ต้องการ + AsNoTracking (ลด memory)
        var workOrders = await db.WorkOrders
            .AsNoTracking()
            .Select(w => new WorkOrderSlim(w.Id, w.Order, w.DefaultLine))
            .ToListAsync(token);

        if (!workOrders.Any()) return;

        _logger.LogInformation("[WorkOrderLineSyncService] 🔄 Syncing {Count} orders", workOrders.Count);

        // ✅ 2. Parallel MES calls ด้วย SemaphoreSlim (จำกัด concurrency)
        var semaphore = new SemaphoreSlim(MaxConcurrency);
        var results = new System.Collections.Concurrent.ConcurrentBag<(int Id, string NewLine)>();

        var tasks = workOrders.Select(async wo =>
        {
            await semaphore.WaitAsync(token);
            try
            {
                var routingData = $"0}}{wo.Order}";
                var raw = await mesClient.CallAsync(testType: "GET_MO_INFO", routingData: routingData);
                var newDefaultLine = ExtractDefaultLine(raw);

                // ✅ เก็บเฉพาะที่เปลี่ยนแปลง
                if (newDefaultLine is not null && newDefaultLine != wo.DefaultLine)
                {
                    results.Add((wo.Id, newDefaultLine));
                    _logger.LogInformation(
                        "[WorkOrderLineSyncService] 📝 Order {Order}: '{Old}' → '{New}'",
                        wo.Order, wo.DefaultLine ?? "(null)", newDefaultLine);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[WorkOrderLineSyncService] ⚠️ Failed for Order {Order}", wo.Order);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        // ✅ 3. Batch update ด้วย ExecuteUpdateAsync (อัพเดทตรงที่ DB ไม่ต้อง track)
        if (results.Any())
        {
            foreach (var (id, newLine) in results)
            {
                await db.WorkOrders
                    .Where(w => w.Id == id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(w => w.DefaultLine, newLine)
                        .SetProperty(w => w.UpdatedDate, DateTime.UtcNow),
                    token);
            }

            _logger.LogInformation(
                "[WorkOrderLineSyncService] ✅ Updated {Count}/{Total} orders in {Elapsed}ms",
                results.Count, workOrders.Count, sw.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation(
                "[WorkOrderLineSyncService] ✅ No changes ({Total} checked in {Elapsed}ms)",
                workOrders.Count, sw.ElapsedMilliseconds);
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
