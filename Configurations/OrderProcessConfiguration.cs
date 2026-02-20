using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations;

public class OrderProcessConfiguration : IEntityTypeConfiguration<OrderProcess>
{
    public void Configure(EntityTypeBuilder<OrderProcess> builder)
    {
        builder.ToTable("OrderProcesses");

        builder.HasKey(op => op.Id);

        builder.HasIndex(op => op.OrderNumber)
               .IsUnique();

        builder.HasIndex(op => op.Status); // ✅ Index for Status
        builder.HasIndex(op => op.CreatedDate); // ✅ Index for CreatedDate

        builder.Property(op => op.OrderNumber)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(op => op.Status)
               .IsRequired()
               .HasMaxLength(20)
               .HasDefaultValue("Pending");

        builder.Property(op => op.CreatedDate)
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // ✅ DestinationStation
        builder.Property(op => op.DestinationStation)
               .IsRequired()
               .HasMaxLength(50)
               .HasDefaultValue("");

        // ✅ TimeToUse: เก็บวันปัจจุบัน + เวลา (ตามที่ Client ส่งมา)
        builder.Property(op => op.TimeToUse)
               .HasColumnType("timestamp with time zone")   // สำหรับ SQL Server/SQLite
               .IsRequired(false);

        // WorkOrder relation
        builder.HasOne(op => op.WorkOrder)
               .WithMany(w => w.OrderProcesses)
               .HasForeignKey(op => op.WorkOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        // User relation (CreatedBy)
        builder.HasOne(op => op.CreatedBy)
               .WithMany()
               .HasForeignKey(op => op.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
               
        // ---------------- OrderMaterial relation (1:N) ----------------
        builder.HasMany(op => op.OrderMaterials)
               .WithOne(om => om.OrderProcess)
               .HasForeignKey(om => om.OrderProcessId)
               .OnDelete(DeleteBehavior.Cascade);

        // Optional processes (1:1)
              builder.HasOne(op => op.ConfirmProcess)
               .WithOne(cp => cp.OrderProcess)
               .HasForeignKey<ConfirmProcess>(cp => cp.OrderProcessId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(op => op.PreparingProcess)
               .WithOne(pp => pp.OrderProcess)
               .HasForeignKey<PreparingProcess>(pp => pp.OrderProcessId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(op => op.ShipmentProcess)
               .WithOne(sp => sp.OrderProcess)
               .HasForeignKey<ShipmentProcess>(sp => sp.OrderProcessId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(op => op.ReceiveProcess)
               .WithOne(rp => rp.OrderProcess)
               .HasForeignKey<ReceivedProcess>(rp => rp.OrderProcessId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(op => op.CancelledProcess)
               .WithOne(cp => cp.OrderProcess)
               .HasForeignKey<CancelledProcess>(cp => cp.OrderProcessId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(op => op.ReturnProcess)
               .WithOne(rp => rp.OrderProcess)
               .HasForeignKey<ReturnProcess>(rp => rp.OrderProcessId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
