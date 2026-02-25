using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WorkOrderApplication.API.Constants;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Enums;
using WorkOrderApplication.API.Hubs;
using WorkOrderApplication.API.Mappings;
using System.Collections.Concurrent;
namespace WorkOrderApplication.API.Services;

public class OrderRecordBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<OrderProcessHub, IOrderClient> _hubContext;
    private readonly ILogger<OrderRecordBackgroundService> _logger;

    private readonly ConcurrentDictionary<int, OrderSnapshot> _cache = new();

    private record OrderSnapshot(int OrderState, int ExecutingIndex, double Progress, DateTime Updated);

    public OrderRecordBackgroundService(
        IServiceScopeFactory scopeFactory,
        IHubContext<OrderProcessHub, IOrderClient> hubContext,
        ILogger<OrderRecordBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[HYBRID 🚀] OrderRecordBackgroundService started");

        _cache.Clear();
        _logger.LogInformation("[HYBRID ♻️] Cache cleared (FullRefresh)");

        var polling = PollingLoop(stoppingToken);
        var fullRefresh = FullRefreshLoop(stoppingToken);

        await Task.WhenAll(polling, fullRefresh);
    }

    private async Task PollingLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await SyncOrdersAsync(incremental: true, token);
            await Task.Delay(TimeSpan.FromSeconds(1), token);
        }
    }

    private async Task FullRefreshLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(5), token);
            await SyncOrdersAsync(incremental: false, token);
        }
    }

    private async Task SyncOrdersAsync(bool incremental, CancellationToken token)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var proxy = scope.ServiceProvider.GetRequiredService<OrderProxyService>();

        var mode = incremental ? "Polling" : "FullRefresh";
        var start = DateTime.UtcNow;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // 🕐 STEP 1: ดึงข้อมูลทั้งหมดจาก External API
            _logger.LogInformation("[{Mode}] ▶️ Step 1: Fetching order records...", mode);
            var response = await proxy.GetOrderRecordsAsync(1, 5000);
            var allOrders = response?.Result?.Records ?? new List<OrderRecordDto>();
            _logger.LogInformation("[{Mode}] ✅ Step 1 done | {Count} fetched | {Elapsed} ms",
                mode, allOrders.Count, sw.ElapsedMilliseconds);

            if (!allOrders.Any())
            {
                _logger.LogWarning("[{Mode}] ⚠️ No orders fetched", mode);
                return;
            }

            sw.Restart();

            // 🧠 STEP 2: โหลด ShipmentProcess ทั้งหมด (เฉพาะ External API mode)
            var shipments = await db.ShipmentProcesses
                .Where(s => s.ShipmentMode == ShipmentMode.ExternalApi)  // ✅ Sync เฉพาะ External API
                .ToListAsync(token);
            var trackedIds = shipments.Select(s => s.ExternalId).ToHashSet();

            // 🔍 Filter เฉพาะ order ที่อยู่ใน shipment
            var orders = allOrders.Where(o => trackedIds.Contains(o.Id)).ToList();

            _logger.LogInformation("[{Mode}] ✅ Step 2 done | Filtered: {Count}/{Total}",
                mode, orders.Count, allOrders.Count);

            if (!orders.Any())
            {
                _logger.LogInformation("[{Mode}] ⚠️ No shipment-related orders to sync", mode);
                return;
            }

            sw.Restart();

            // ⚙️ STEP 3: Update OrderRecord + Mirror ShipmentProcess
            int newCount = 0, changedCount = 0, unchangedCount = 0;
            var changedOrders = new List<OrderRecordDto>();

            foreach (var order in orders)
            {
                var snapshot = new OrderSnapshot(order.OrderState, order.ExecutingIndex, order.Progress, DateTime.UtcNow);

                // 🚫 Skip completed orders
                if (order.OrderState == 2 || order.OrderState == 5)
                {
                    _cache[order.Id] = snapshot;
                    continue;
                }

                bool isChanged = false;

                // 🆕 New record
                if (!_cache.TryGetValue(order.Id, out var old))
                {
                    _cache[order.Id] = snapshot;
                    newCount++;
                    isChanged = true;
                    db.OrderRecords.Add(order.ToOrderRecord());
                }
                // 🔄 Changed record
                else if (old.OrderState != snapshot.OrderState ||
                         old.ExecutingIndex != snapshot.ExecutingIndex ||
                         Math.Abs(old.Progress - snapshot.Progress) > 0.001)
                {
                    _cache[order.Id] = snapshot;
                    changedCount++;
                    isChanged = true;

                    var record = await db.OrderRecords.FirstOrDefaultAsync(x => x.Id == order.Id, token);
                    if (record is not null)
                    {
                        record.LastStatus = order.OrderState.ToString();
                        record.ExecutingIndex = order.ExecutingIndex;
                        record.Progress = order.Progress;
                        record.LastUpdated = DateTime.UtcNow;

                        var jsonOptions = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };

                        record.RawResponse = JsonSerializer.Serialize(order, jsonOptions);
                    }
                }
                else
                {
                    unchangedCount++;
                }

                // ✅ Mirror Logic: ถ้ามีการเปลี่ยน → update ShipmentProcess
                if (isChanged)
                {
                    var shipment = shipments.FirstOrDefault(s => s.ExternalId == order.Id);
                    if (shipment is not null)
                    {
                        shipment.OrderState = order.OrderState;
                        shipment.ExecutingIndex = order.ExecutingIndex;
                        shipment.Progress = order.Progress;
                        shipment.LastSynced = DateTime.UtcNow;
                        shipment.ExecuteVehicleName = order.ExecuteVehicleName ?? shipment.ExecuteVehicleName;
                        shipment.ExecuteVehicleKey = order.ExecuteVehicleKey ?? shipment.ExecuteVehicleKey;
                    }

                    changedOrders.Add(order);
                }
            }

            _logger.LogInformation("[{Mode}] ✅ Step 3 done | Compare cache: {Elapsed} ms",
                mode, sw.ElapsedMilliseconds);

            sw.Restart();

            // 🧹 STEP 4: Cache cleanup (FullRefresh)
            if (!incremental)
            {
                var activeIds = orders.Select(o => o.Id).ToHashSet();
                var removed = _cache.Keys.Where(id => !activeIds.Contains(id)).ToList();
                foreach (var id in removed)
                    _cache.TryRemove(id, out _);
                _logger.LogInformation("[{Mode}] ♻️ Cache cleaned: {Count} removed", mode, removed.Count);
            }

            // 💾 STEP 5: Save to DB
            if (changedOrders.Any())
            {
                await db.SaveChangesAsync(token);
            }

            _logger.LogInformation("[{Mode}] ✅ Step 4 done | SaveChangesAsync: {Elapsed} ms",
                mode, sw.ElapsedMilliseconds);

            if (changedOrders.Any())
            {
                await _hubContext.Clients.Group(SignalRGroups.AllOrders).OrderRecordUpdated(changedOrders);
                _logger.LogInformation("[{Mode}] 📡 Broadcasted updates ({Count})", mode, changedOrders.Count);
            }

            // 🧾 STEP 7: Summary log
            var duration = (DateTime.UtcNow - start).TotalMilliseconds;
            _logger.LogInformation("[HYBRID 🛰] {Mode} complete | New: {New} | Changed: {Changed} | Unchanged: {Unchanged} | Duration: {Duration} ms",
                mode, newCount, changedCount, unchangedCount, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HYBRID ❌] Error in {Mode}", mode);
        }
    }
}
