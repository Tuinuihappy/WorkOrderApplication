using Microsoft.EntityFrameworkCore;
using WorkOrderApplication.API.Configurations;
using WorkOrderApplication.API.Entities;
using System.Reflection;
namespace WorkOrderApplication.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<User> Users => Set<User>();
    public DbSet<OrderProcess> OrderProcesses => Set<OrderProcess>();
    public DbSet<OrderMaterial> OrderMaterials => Set<OrderMaterial>();
    public DbSet<ConfirmProcess> ConfirmProcesses => Set<ConfirmProcess>();
    public DbSet<PreparingProcess> PreparingProcesses => Set<PreparingProcess>();
    public DbSet<PreparingMaterial> PreparingMaterials => Set<PreparingMaterial>();
    public DbSet<ShipmentProcess> ShipmentProcesses => Set<ShipmentProcess>();
    public DbSet<ReceivedProcess> ReceivedProcesses => Set<ReceivedProcess>();
    public DbSet<ReceivedMaterial> ReceivedMaterials => Set<ReceivedMaterial>();
    public DbSet<CancelledProcess> CancelledProcesses => Set<CancelledProcess>();
    public DbSet<ReturnProcess> ReturnProcesses => Set<ReturnProcess>();
    public DbSet<OrderGroupAMR> OrderGroupAMRs => Set<OrderGroupAMR>();
    public DbSet<OrderRecord> OrderRecords => Set<OrderRecord>();
    public DbSet<OrderRecordById> OrderRecordByIds => Set<OrderRecordById>();
    public DbSet<OrderMission> OrderMissions => Set<OrderMission>();
    
    // ✅ เพิ่ม DbSet สำหรับ PushSubscriptionEntity
    public DbSet<PushSubscriptionEntity> Subscriptions => Set<PushSubscriptionEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ✅ โหลด configuration ทั้งหมดจาก assembly ปัจจุบัน
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
