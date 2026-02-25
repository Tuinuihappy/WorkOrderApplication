// using Microsoft.AspNetCore.SignalR;
// using Microsoft.EntityFrameworkCore;
// using System.Collections.Concurrent;
// using System.Text.Json;
// using WorkOrderApplication.API.Data;
// using WorkOrderApplication.API.Dtos;
// using WorkOrderApplication.API.Entities;
// using WorkOrderApplication.API.Hubs;
// using WorkOrderApplication.API.Mappings;

// namespace WorkOrderApplication.API.Services;

// public class OrderRecordByIdBackgroundService : BackgroundService
// {
//     private readonly IServiceScopeFactory _scopeFactory;
//     private readonly IHubContext<OrderRecordHub> _hubContext;
//     private readonly ILogger<OrderRecordByIdBackgroundService> _logger;

//     private readonly ConcurrentDictionary<int, OrderSnapshot> _cache = new();

//     private record OrderSnapshot(int OrderState, int ExecutingIndex, double Progress, DateTime Updated);

//     public OrderRecordByIdBackgroundService(
//         IServiceScopeFactory scopeFactory,
//         IHubContext<OrderRecordHub> hubContext,
//         ILogger<OrderRecordByIdBackgroundService> logger)
//     {
//         _scopeFactory = scopeFactory;
//         _hubContext = hubContext;
//         _logger = logger;
//     }

//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         _logger.LogInformation("[DETAIL 🚀] OrderRecordByIdBackgroundService started");
//         _cache.Clear();

//         while (!stoppingToken.IsCancellationRequested)
//         {
//             await SyncDetailedOrdersAsync(stoppingToken);
//             await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
//         }
//     }

//     private async Task SyncDetailedOrdersAsync(CancellationToken token)
//     {
//         await using var scope = _scopeFactory.CreateAsyncScope();
//         var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//         var proxy = scope.ServiceProvider.GetRequiredService<OrderProxyService>();
//         var notifier = scope.ServiceProvider.GetRequiredService<OrderProcessNotifier>();

//         var start = DateTime.UtcNow;
//         var sw = System.Diagnostics.Stopwatch.StartNew();

//         try
//         {
//             // STEP 1: โหลดรายการ ShipmentProcesses
//             var tracked = await db.ShipmentProcesses.ToListAsync(token);
//             if (!tracked.Any())
//             {
//                 _logger.LogDebug("[DETAIL ⚠️] No tracked orders found.");
//                 return;
//             }

//             // STEP 2: Filter เฉพาะ order ที่ยังไม่จบ (state != 2/5)
//             var activeOrders = tracked
//                 .Where(t => !_cache.TryGetValue(t.ExternalId, out var snap) ||
//                             (snap.OrderState != 2 && snap.OrderState != 5))
//                 .ToList();

//             if (!activeOrders.Any())
//             {
//                 _logger.LogInformation("[DETAIL ⏭] All tracked orders are in final state (2/5). Skipping fetch.");
//                 return;
//             }

//             // STEP 3: Fetch รายละเอียดจาก RIOT API
//             var results = new ConcurrentBag<(ShipmentProcess, OrderRecordByIdDto?)>();
//             await Parallel.ForEachAsync(activeOrders, new ParallelOptions
//             {
//                 MaxDegreeOfParallelism = 5,
//                 CancellationToken = token
//             }, async (t, ct) =>
//             {
//                 var detail = await proxy.GetOrderRecordByIdAsync(t.ExternalId);
//                 results.Add((t, detail));
//             });

//             sw.Stop();
//             _logger.LogInformation("[DETAIL ✅] API calls done ({Count}) in {Elapsed} ms", results.Count, sw.ElapsedMilliseconds);

//             // STEP 4: ประมวลผล
//             int newCount = 0, changedCount = 0, unchangedCount = 0;
//             var changedOrders = new List<OrderRecordByIdDto>();

//             foreach (var (trackedOrder, dto) in results)
//             {
//                 if (dto is null) continue;
//                 var order = dto;

//                 var snapshot = new OrderSnapshot(order.OrderState, order.ExecutingIndex, order.Progress, DateTime.UtcNow);
//                 bool isChanged = false;

//                 // ➕ INSERT ใหม่
//                 if (!_cache.TryGetValue(trackedOrder.ExternalId, out var old))
//                 {
//                     _cache[trackedOrder.ExternalId] = snapshot;
//                     isChanged = true;
//                     newCount++;
//                     db.OrderRecordByIds.Add(order.ToOrderRecordById());
//                 }
//                 // 🔁 UPDATE เดิม
//                 else if (old.OrderState != snapshot.OrderState ||
//                          old.ExecutingIndex != snapshot.ExecutingIndex ||
//                          Math.Abs(old.Progress - snapshot.Progress) > 0.001)
//                 {
//                     _cache[trackedOrder.ExternalId] = snapshot;
//                     isChanged = true;
//                     changedCount++;

//                     var existing = await db.OrderRecordByIds.FirstOrDefaultAsync(x => x.Id == order.Id, token);
//                     if (existing is not null)
//                     {
//                         existing.OrderState = order.OrderState;
//                         existing.ExecutingIndex = order.ExecutingIndex;
//                         existing.Progress = order.Progress;
//                         existing.UpdateTime = order.UpdateTime;

//                         existing.RawResponse = JsonSerializer.Serialize(order, new JsonSerializerOptions
//                         {
//                             WriteIndented = true,
//                             PropertyNamingPolicy = JsonNamingPolicy.CamelCase
//                         });
//                     }
//                 }
//                 else
//                 {
//                     unchangedCount++;
//                 }

//                 // ✅ Mirror: ShipmentProcess
//                 if (isChanged)
//                 {
//                     var shipment = await db.ShipmentProcesses
//                         .FirstOrDefaultAsync(s => s.ExternalId == order.Id, token);

//                     if (shipment is not null)
//                     {
//                         shipment.OrderState = order.OrderState;
//                         shipment.ExecutingIndex = order.ExecutingIndex;
//                         shipment.Progress = order.Progress;
//                         shipment.LastSynced = DateTime.UtcNow;
//                         shipment.ExecuteVehicleName = order.ExecuteVehicleName ?? shipment.ExecuteVehicleName;
//                         shipment.ExecuteVehicleKey = order.ExecuteVehicleKey ?? shipment.ExecuteVehicleKey;
//                     }

//                     changedOrders.Add(order);
//                 }

//                 if (order.OrderState == 2 || order.OrderState == 5)
//                 {
//                     _cache[trackedOrder.ExternalId] = snapshot;
//                 }
//             }

//             // ✅ STEP 5: Full Mirror Mission Sync (พร้อมแก้ ref & scope error)
//             foreach (var changed in changedOrders)
//             {
//                 if (changed.Missions is null || changed.Missions.Count == 0) continue;

//                 var orderRecord = await db.OrderRecordByIds
//                     .FirstOrDefaultAsync(o => o.Id == changed.Id, token);
//                 if (orderRecord is null) continue;

//                 // 🧭 1. ดึง id จาก response ล่าสุดของ RIOT
//                 var currentMissionIds = changed.Missions.Select(m => m.Id).ToHashSet();

//                 // 🧹 2. ดึง missions ที่มีใน DB ปัจจุบัน
//                 var existingMissions = await db.OrderMissions
//                     .Where(m => m.OrderRecordByIdId == orderRecord.Id)
//                     .ToListAsync(token);

//                 // 🗑 3. ลบ mission ที่ไม่มีใน response ล่าสุด
//                 var missionsToDelete = existingMissions
//                     .Where(m => !currentMissionIds.Contains(m.Id))
//                     .ToList();

//                 if (missionsToDelete.Any())
//                 {
//                     db.OrderMissions.RemoveRange(missionsToDelete);
//                     _logger.LogWarning("[DETAIL 🗑] Removed {Count} obsolete missions for OrderRecord={OrderId}",
//                         missionsToDelete.Count, orderRecord.Id);
//                 }

//                 // 🔁 4. Sync mission ที่เหลืออยู่ (เพิ่มใหม่หรืออัปเดตเดิม)
//                 foreach (var missionDto in changed.Missions)
//                 {
//                     var existingMission = existingMissions.FirstOrDefault(m => m.Id == missionDto.Id);
//                     bool changedMission = false;

//                     void UpdateIfChanged<T>(T currentValue, T newValue, Action<T> setter, string fieldName)
//                     {
//                         if (!EqualityComparer<T>.Default.Equals(currentValue, newValue))
//                         {
//                             setter(newValue);
//                             changedMission = true;
//                             _logger.LogDebug("[DETAIL ⚙️] Mission field {Field} updated", fieldName);
//                         }
//                     }

//                     if (existingMission is null)
//                     {
//                         var mission = new OrderMission
//                         {
//                             Id = missionDto.Id,
//                             MissionState = missionDto.MissionState,
//                             ExecutingIndex = missionDto.ExecutingIndex,
//                             Type = missionDto.Type,
//                             ActionName = missionDto.ActionName,
//                             Destination = missionDto.Destination,
//                             DestinationName = missionDto.DestinationName,
//                             MapName = missionDto.MapName,
//                             ResultCode = missionDto.ResultCode,
//                             ResultStr = missionDto.ResultStr,
//                             CreateTime = missionDto.CreateTime,
//                             ExecuteTime = missionDto.ExecuteTime,
//                             FinishTime = missionDto.FinishTime,
//                             OrderRecordByIdId = orderRecord.Id
//                         };

//                         db.OrderMissions.Add(mission);
//                         _logger.LogInformation("[DETAIL ➕] Mission added: {MissionId} for OrderRecord={OrderId}",
//                             mission.Id, orderRecord.Id);
//                     }
//                     else
//                     {
//                         UpdateIfChanged(existingMission.MissionState, missionDto.MissionState, v => existingMission.MissionState = v, nameof(existingMission.MissionState));
//                         UpdateIfChanged(existingMission.ExecutingIndex, missionDto.ExecutingIndex, v => existingMission.ExecutingIndex = v, nameof(existingMission.ExecutingIndex));
//                         UpdateIfChanged(existingMission.Type, missionDto.Type, v => existingMission.Type = v, nameof(existingMission.Type));
//                         UpdateIfChanged(existingMission.ActionName, missionDto.ActionName, v => existingMission.ActionName = v, nameof(existingMission.ActionName));
//                         UpdateIfChanged(existingMission.Destination, missionDto.Destination, v => existingMission.Destination = v, nameof(existingMission.Destination));
//                         UpdateIfChanged(existingMission.DestinationName, missionDto.DestinationName, v => existingMission.DestinationName = v, nameof(existingMission.DestinationName));
//                         UpdateIfChanged(existingMission.MapName, missionDto.MapName, v => existingMission.MapName = v, nameof(existingMission.MapName));
//                         UpdateIfChanged(existingMission.ResultCode, missionDto.ResultCode, v => existingMission.ResultCode = v, nameof(existingMission.ResultCode));
//                         UpdateIfChanged(existingMission.ResultStr, missionDto.ResultStr, v => existingMission.ResultStr = v, nameof(existingMission.ResultStr));
//                         UpdateIfChanged(existingMission.CreateTime, missionDto.CreateTime, v => existingMission.CreateTime = v, nameof(existingMission.CreateTime));
//                         UpdateIfChanged(existingMission.ExecuteTime, missionDto.ExecuteTime, v => existingMission.ExecuteTime = v, nameof(existingMission.ExecuteTime));
//                         UpdateIfChanged(existingMission.FinishTime, missionDto.FinishTime, v => existingMission.FinishTime = v, nameof(existingMission.FinishTime));

//                         if (changedMission)
//                         {
//                             _logger.LogInformation("[DETAIL 🔄] Mission updated: {MissionId} for OrderRecord={OrderId}",
//                                 missionDto.Id, orderRecord.Id);
//                         }
//                     }
//                 }

//                 // 📡 5. Broadcast missions ปัจจุบันทั้งหมด
//                 var syncedMissions = await db.OrderMissions
//                     .Where(m => m.OrderRecordByIdId == orderRecord.Id)
//                     .OrderBy(m => m.ExecutingIndex)
//                     .ToListAsync(token);

//                 await _hubContext.Clients.Group($"order-{orderRecord.Id}")
//                     .SendAsync("OrderMissionsUpdated", syncedMissions, token);

//                 // 📦 STEP 5.1: ตรวจสอบ Mission สำเร็จปลายทาง → อัปเดต OrderProcess.Status
//                 var shipmentProcess = await db.ShipmentProcesses
//                     .FirstOrDefaultAsync(s => s.ExternalId == changed.Id, token);

//                 if (shipmentProcess != null)
//                 {
//                     var matchedMission = syncedMissions.FirstOrDefault(m =>
//                         m.MissionState == 2 &&
//                         m.Destination == shipmentProcess.DestinationStationId);

//                     if (matchedMission != null)
//                     {
//                         var orderProcess = await db.OrderProcesses
//                             .FirstOrDefaultAsync(op => op.Id == shipmentProcess.OrderProcessId, token);

//                         if (orderProcess != null && orderProcess.Status == "Shipment")
//                         {
//                             orderProcess.Status = "Awaiting Pickup";
                            
//                             // 🕓 เก็บเวลา ArrivalTime ตอนถึงปลายทาง
//                             shipmentProcess.ArrivalTime = DateTime.UtcNow;
//                             shipmentProcess.LastSynced = DateTime.UtcNow;
//                             await db.SaveChangesAsync(token);

//                             _logger.LogInformation(
//                                 "[DETAIL 📦] OrderProcess {OrderProcessId} status changed → Awaiting Pickup (from Shipment, Mission={MissionId})",
//                                 orderProcess.Id, matchedMission.Id);
//                             // ✅ โหลดใหม่ เพื่อให้ ToDetailsDto ได้ข้อมูลล่าสุด
//                             var updated = await db.OrderProcesses
//                                 .Include(o => o.WorkOrder)
//                                 .Include(o => o.CreatedBy)
//                                 .FirstOrDefaultAsync(o => o.Id == orderProcess.Id, token);

//                             if (updated != null)
//                             {
//                                 await notifier.BroadcastUpdatedAsync(updated.Id, updated.ToDetailsDto());
//                             }
//                         }
//                     }
//                 }
//             }

//             // ✅ STEP 6: Commit + Broadcast shipment
//             if (changedOrders.Any())
//             {
//                 await db.SaveChangesAsync(token);

//                 foreach (var changed in changedOrders)
//                 {
//                     var shipment = await db.ShipmentProcesses
//                         .FirstOrDefaultAsync(s => s.ExternalId == changed.Id, token);
//                     if (shipment is not null)
//                     {
//                         await notifier.BroadcastShipmentStateChangedAsync(shipment);
//                     }
//                 }
//             }

//             _logger.LogInformation("[DETAIL 🛰] Done | New={New} | Changed={Changed} | Unchanged={Unchanged} | Duration={Duration} ms",
//                 newCount, changedCount, unchangedCount, (DateTime.UtcNow - start).TotalMilliseconds);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "[DETAIL ❌] Error while syncing detailed orders");
//         }
//     }
// }



using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Text.Json;
using WorkOrderApplication.API.Constants;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Enums;
using WorkOrderApplication.API.Hubs;
using WorkOrderApplication.API.Mappings;

namespace WorkOrderApplication.API.Services;

public class OrderRecordByIdBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<OrderProcessHub, IOrderClient> _hubContext;
    private readonly ILogger<OrderRecordByIdBackgroundService> _logger;

    private readonly ConcurrentDictionary<int, OrderSnapshot> _cache = new();

    private record OrderSnapshot(int OrderState, int ExecutingIndex, double Progress, DateTime Updated);

    public OrderRecordByIdBackgroundService(
        IServiceScopeFactory scopeFactory,
        IHubContext<OrderProcessHub, IOrderClient> hubContext,
        ILogger<OrderRecordByIdBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[DETAIL 🚀] OrderRecordByIdBackgroundService started");
        _cache.Clear();

        while (!stoppingToken.IsCancellationRequested)
        {
            await SyncDetailedOrdersAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task SyncDetailedOrdersAsync(CancellationToken token)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var proxy = scope.ServiceProvider.GetRequiredService<OrderProxyService>();
        var notifier = scope.ServiceProvider.GetRequiredService<OrderProcessNotifier>();

        var start = DateTime.UtcNow;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // STEP 1: โหลดรายการ ShipmentProcesses (เฉพาะ External API mode)
            var tracked = await db.ShipmentProcesses
                .Where(s => s.ShipmentMode == ShipmentMode.ExternalApi)  // ✅ Sync เฉพาะ External API
                .ToListAsync(token);
            if (!tracked.Any())
            {
                _logger.LogDebug("[DETAIL ⚠️] No tracked orders found.");
                return;
            }

            // STEP 2: Filter เฉพาะ order ที่ยังไม่จบ (state != 2/5)
            var activeOrders = tracked
                .Where(t => !_cache.TryGetValue(t.ExternalId, out var snap) ||
                            (snap.OrderState != 2 && snap.OrderState != 5))
                .ToList();

            if (!activeOrders.Any())
            {
                _logger.LogInformation("[DETAIL ⏭] All tracked orders are in final state (2/5). Skipping fetch.");
                return;
            }

            // STEP 3: Fetch รายละเอียดจาก RIOT API
            var results = new ConcurrentBag<(ShipmentProcess, OrderRecordByIdDto?)>();
            await Parallel.ForEachAsync(activeOrders, new ParallelOptions
            {
                MaxDegreeOfParallelism = 5,
                CancellationToken = token
            }, async (t, ct) =>
            {
                var detail = await proxy.GetOrderRecordByIdAsync(t.ExternalId);
                results.Add((t, detail));
            });

            sw.Stop();
            _logger.LogInformation("[DETAIL ✅] API calls done ({Count}) in {Elapsed} ms", results.Count, sw.ElapsedMilliseconds);

            // STEP 4: ประมวลผล
            int newCount = 0, changedCount = 0, unchangedCount = 0;
            var changedOrders = new List<OrderRecordByIdDto>();

            foreach (var (trackedOrder, dto) in results)
            {
                if (dto is null) continue;
                var order = dto;

                var snapshot = new OrderSnapshot(order.OrderState, order.ExecutingIndex, order.Progress, DateTime.UtcNow);
                bool isChanged = false;

                // ➕ INSERT ใหม่
                if (!_cache.TryGetValue(trackedOrder.ExternalId, out var old))
                {
                    _cache[trackedOrder.ExternalId] = snapshot;
                    isChanged = true;
                    newCount++;
                    db.OrderRecordByIds.Add(order.ToOrderRecordById());
                }
                // 🔁 UPDATE เดิม
                else if (old.OrderState != snapshot.OrderState ||
                         old.ExecutingIndex != snapshot.ExecutingIndex ||
                         Math.Abs(old.Progress - snapshot.Progress) > 0.001)
                {
                    _cache[trackedOrder.ExternalId] = snapshot;
                    isChanged = true;
                    changedCount++;

                    var existing = await db.OrderRecordByIds.FirstOrDefaultAsync(x => x.Id == order.Id, token);
                    if (existing is not null)
                    {
                        existing.OrderState = order.OrderState;
                        existing.ExecutingIndex = order.ExecutingIndex;
                        existing.Progress = order.Progress;
                        existing.UpdateTime = order.UpdateTime;

                        existing.RawResponse = JsonSerializer.Serialize(order, new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                    }
                }
                else
                {
                    unchangedCount++;
                }

                // ✅ Mirror: ShipmentProcess
                if (isChanged)
                {
                    var shipment = await db.ShipmentProcesses
                        .FirstOrDefaultAsync(s => s.ExternalId == order.Id, token);

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

                if (order.OrderState == 2 || order.OrderState == 5)
                {
                    _cache[trackedOrder.ExternalId] = snapshot;
                }
            }

            // ✅ STEP 5: Full Mirror Mission Sync (พร้อมแก้ ref & scope error)
            foreach (var changed in changedOrders)
            {
                if (changed.Missions is null || changed.Missions.Count == 0) continue;

                var orderRecord = await db.OrderRecordByIds
                    .FirstOrDefaultAsync(o => o.Id == changed.Id, token);
                if (orderRecord is null) continue;

                // 🧭 1. ดึง id จาก response ล่าสุดของ RIOT
                var currentMissionIds = changed.Missions.Select(m => m.Id).ToHashSet();

                // 🧹 2. ดึง missions ที่มีใน DB ปัจจุบัน
                var existingMissions = await db.OrderMissions
                    .Where(m => m.OrderRecordByIdId == orderRecord.Id)
                    .ToListAsync(token);

                // 🗑 3. ลบ mission ที่ไม่มีใน response ล่าสุด
                var missionsToDelete = existingMissions
                    .Where(m => !currentMissionIds.Contains(m.Id))
                    .ToList();

                if (missionsToDelete.Any())
                {
                    db.OrderMissions.RemoveRange(missionsToDelete);
                    _logger.LogWarning("[DETAIL 🗑] Removed {Count} obsolete missions for OrderRecord={OrderId}",
                        missionsToDelete.Count, orderRecord.Id);
                }

                // 🔁 4. Sync mission ที่เหลืออยู่ (เพิ่มใหม่หรืออัปเดตเดิม)
                foreach (var missionDto in changed.Missions)
                {
                    var existingMission = existingMissions.FirstOrDefault(m => m.Id == missionDto.Id);
                    bool changedMission = false;

                    void UpdateIfChanged<T>(T currentValue, T newValue, Action<T> setter, string fieldName)
                    {
                        if (!EqualityComparer<T>.Default.Equals(currentValue, newValue))
                        {
                            setter(newValue);
                            changedMission = true;
                            _logger.LogDebug("[DETAIL ⚙️] Mission field {Field} updated", fieldName);
                        }
                    }

                    if (existingMission is null)
                    {
                        var mission = new OrderMission
                        {
                            Id = missionDto.Id,
                            MissionState = missionDto.MissionState,
                            ExecutingIndex = missionDto.ExecutingIndex,
                            Type = missionDto.Type,
                            ActionName = missionDto.ActionName,
                            Destination = missionDto.Destination,
                            DestinationName = missionDto.DestinationName,
                            MapName = missionDto.MapName,
                            ResultCode = missionDto.ResultCode,
                            ResultStr = missionDto.ResultStr,
                            CreateTime = missionDto.CreateTime,
                            ExecuteTime = missionDto.ExecuteTime,
                            FinishTime = missionDto.FinishTime,
                            OrderRecordByIdId = orderRecord.Id
                        };

                        db.OrderMissions.Add(mission);
                        _logger.LogInformation("[DETAIL ➕] Mission added: {MissionId} for OrderRecord={OrderId}",
                            mission.Id, orderRecord.Id);
                    }
                    else
                    {
                        UpdateIfChanged(existingMission.MissionState, missionDto.MissionState, v => existingMission.MissionState = v, nameof(existingMission.MissionState));
                        UpdateIfChanged(existingMission.ExecutingIndex, missionDto.ExecutingIndex, v => existingMission.ExecutingIndex = v, nameof(existingMission.ExecutingIndex));
                        UpdateIfChanged(existingMission.Type, missionDto.Type, v => existingMission.Type = v, nameof(existingMission.Type));
                        UpdateIfChanged(existingMission.ActionName, missionDto.ActionName, v => existingMission.ActionName = v, nameof(existingMission.ActionName));
                        UpdateIfChanged(existingMission.Destination, missionDto.Destination, v => existingMission.Destination = v, nameof(existingMission.Destination));
                        UpdateIfChanged(existingMission.DestinationName, missionDto.DestinationName, v => existingMission.DestinationName = v, nameof(existingMission.DestinationName));
                        UpdateIfChanged(existingMission.MapName, missionDto.MapName, v => existingMission.MapName = v, nameof(existingMission.MapName));
                        UpdateIfChanged(existingMission.ResultCode, missionDto.ResultCode, v => existingMission.ResultCode = v, nameof(existingMission.ResultCode));
                        UpdateIfChanged(existingMission.ResultStr, missionDto.ResultStr, v => existingMission.ResultStr = v, nameof(existingMission.ResultStr));
                        UpdateIfChanged(existingMission.CreateTime, missionDto.CreateTime, v => existingMission.CreateTime = v, nameof(existingMission.CreateTime));
                        UpdateIfChanged(existingMission.ExecuteTime, missionDto.ExecuteTime, v => existingMission.ExecuteTime = v, nameof(existingMission.ExecuteTime));
                        UpdateIfChanged(existingMission.FinishTime, missionDto.FinishTime, v => existingMission.FinishTime = v, nameof(existingMission.FinishTime));

                        if (changedMission)
                        {
                            _logger.LogInformation("[DETAIL 🔄] Mission updated: {MissionId} for OrderRecord={OrderId}",
                                missionDto.Id, orderRecord.Id);
                        }
                    }
                }

                // 📡 5. Broadcast missions ปัจจุบันทั้งหมด
                var syncedMissions = await db.OrderMissions
                    .Where(m => m.OrderRecordByIdId == orderRecord.Id)
                    .OrderBy(m => m.ExecutingIndex)
                    .ToListAsync(token);

                var shipmentProcess = await db.ShipmentProcesses
                    .FirstOrDefaultAsync(s => s.ExternalId == changed.Id, token);

                if (shipmentProcess != null)
                {
                    await _hubContext.Clients.Group(SignalRGroups.OrderDetails(shipmentProcess.OrderProcessId))
                        .OrderMissionsUpdated(syncedMissions);
                }

                // 📦 STEP 5.1: ตรวจสอบ Mission สำเร็จปลายทาง → อัปเดต OrderProcess.Status
                if (shipmentProcess != null)
                {
                    var matchedMission = syncedMissions.FirstOrDefault(m =>
                        m.MissionState == 2 &&
                        m.Destination == shipmentProcess.DestinationStationId);

                    if (matchedMission != null)
                    {
                        var orderProcess = await db.OrderProcesses
                            .FirstOrDefaultAsync(op => op.Id == shipmentProcess.OrderProcessId, token);

                        if (orderProcess != null && orderProcess.Status == "In Transit")
                        {
                            orderProcess.Status = "Awaiting Pickup";
                            
                            // 🕓 เก็บเวลา ArrivalTime ตอนถึงปลายทาง
                            shipmentProcess.ArrivalTime = DateTime.UtcNow;
                            shipmentProcess.LastSynced = DateTime.UtcNow;
                            await db.SaveChangesAsync(token);

                            _logger.LogInformation(
                                "[DETAIL 📦] OrderProcess {OrderProcessId} status changed → Awaiting Pickup (from Shipment, Mission={MissionId})",
                                orderProcess.Id, matchedMission.Id);
                            // ✅ โหลดใหม่ เพื่อให้ ToDetailsDto ได้ข้อมูลล่าสุด
                            var updated = await db.OrderProcesses
                                .Include(o => o.WorkOrder)
                                .Include(o => o.CreatedBy)
                                .FirstOrDefaultAsync(o => o.Id == orderProcess.Id, token);

                            if (updated != null)
                            {
                                await notifier.BroadcastUpdatedAsync(updated.Id, updated.ToDetailsDto());
                            }
                        }
                    }
                }
            }

            // ✅ STEP 6: Commit + Broadcast shipment
            if (changedOrders.Any())
            {
                await db.SaveChangesAsync(token);

                foreach (var changed in changedOrders)
                {
                    var shipment = await db.ShipmentProcesses
                        .FirstOrDefaultAsync(s => s.ExternalId == changed.Id, token);
                    if (shipment is not null)
                    {
                        await notifier.BroadcastShipmentStateChangedAsync(shipment);
                    }
                }
            }

            _logger.LogInformation("[DETAIL 🛰] Done | New={New} | Changed={Changed} | Unchanged={Unchanged} | Duration={Duration} ms",
                newCount, changedCount, unchangedCount, (DateTime.UtcNow - start).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DETAIL ❌] Error while syncing detailed orders");
        }
    }
}

