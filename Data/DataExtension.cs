using Microsoft.EntityFrameworkCore;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Data;

public static class DataExtension
{
    public static async Task MigrateDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        await SeedDataAsync(dbContext);
    }

    private static async Task SeedDataAsync(AppDbContext dbContext)
    {
        // ────────────────── Seed Users ──────────────────
        if (!await dbContext.Users.AnyAsync())
        {
            var users = new List<User>
            {
                new() { UserName = "สมชาย ใจดี",       EmployeeId = "EMP001", Position = "Operator",   Department = "Production", Shift = "Day",   Email = "somchai@company.com",  ContactNumber = "081-111-1111" },
                new() { UserName = "สมหญิง รักงาน",     EmployeeId = "EMP002", Position = "Supervisor", Department = "Production", Shift = "Day",   Email = "somying@company.com",  ContactNumber = "081-222-2222" },
                new() { UserName = "ประยุทธ์ ทำดี",      EmployeeId = "EMP003", Position = "Engineer",   Department = "Quality",    Shift = "Night", Email = "prayuth@company.com",  ContactNumber = "081-333-3333" },
                new() { UserName = "วิชัย สร้างสรรค์",    EmployeeId = "EMP004", Position = "Operator",   Department = "Warehouse",  Shift = "Day",   Email = "wichai@company.com",   ContactNumber = "081-444-4444" },
                new() { UserName = "นภา ส่องแสง",       EmployeeId = "EMP005", Position = "Supervisor", Department = "Warehouse",  Shift = "Night", Email = "napa@company.com",     ContactNumber = "081-555-5555" },
            };
            dbContext.Users.AddRange(users);
            await dbContext.SaveChangesAsync();
        }

        // ────────────────── Seed WorkOrders ──────────────────
        if (!await dbContext.WorkOrders.AnyAsync())
        {
            var workOrders = new List<WorkOrder>
            {
                new() { Order = "WO-2026-001", OrderType = "ZP01", Plant = "1100", Material = "PROD-A100", Quantity = 500,  Unit = "PCE", BasicFinishDate = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc), CreatedDate = DateTime.UtcNow },
                new() { Order = "WO-2026-002", OrderType = "ZP02", Plant = "1100", Material = "PROD-B200", Quantity = 300,  Unit = "PCE", BasicFinishDate = new DateTime(2026, 3, 20, 0, 0, 0, DateTimeKind.Utc), CreatedDate = DateTime.UtcNow },
                new() { Order = "WO-2026-003", OrderType = "ZP01", Plant = "1200", Material = "PROD-C300", Quantity = 1000, Unit = "KG",  BasicFinishDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),  CreatedDate = DateTime.UtcNow },
            };
            dbContext.WorkOrders.AddRange(workOrders);
            await dbContext.SaveChangesAsync();

            // ────────────────── Seed Materials (ต้อง seed หลัง WorkOrders เพราะมี FK) ──────────────────
            var wo1 = workOrders[0];
            var wo2 = workOrders[1];
            var wo3 = workOrders[2];

            var materials = new List<Material>
            {
                new() { MaterialNumber = "MAT-001", MaterialDescription = "สกรูหัวกลม M5x10",  ReqmntQty = 1000, QtyWthdrn = 0, BUn = "PCE", WorkOrderId = wo1.Id },
                new() { MaterialNumber = "MAT-002", MaterialDescription = "น็อตหกเหลี่ยม M5",   ReqmntQty = 1000, QtyWthdrn = 0, BUn = "PCE", WorkOrderId = wo1.Id },
                new() { MaterialNumber = "MAT-003", MaterialDescription = "แผ่นเหล็ก 2mm",      ReqmntQty = 50,   QtyWthdrn = 0, BUn = "SHT", WorkOrderId = wo2.Id },
                new() { MaterialNumber = "MAT-004", MaterialDescription = "ท่อทองแดง 15mm",     ReqmntQty = 200,  QtyWthdrn = 0, BUn = "MTR", WorkOrderId = wo2.Id },
                new() { MaterialNumber = "MAT-005", MaterialDescription = "ผงเคมี A-Grade",     ReqmntQty = 500,  QtyWthdrn = 0, BUn = "KG",  WorkOrderId = wo3.Id },
                new() { MaterialNumber = "MAT-006", MaterialDescription = "สารเคลือบ UV",       ReqmntQty = 100,  QtyWthdrn = 0, BUn = "LTR", WorkOrderId = wo3.Id },
            };
            dbContext.Materials.AddRange(materials);
            await dbContext.SaveChangesAsync();
        }
    }
}
