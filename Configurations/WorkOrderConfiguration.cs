using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        // -------------------- Table --------------------
        builder.ToTable("WorkOrders");

        // -------------------- Primary Key --------------------
        builder.HasKey(w => w.Id);

        // -------------------- Properties --------------------
        builder.Property(w => w.WorkOrderNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(w => w.WorkOrderNumber)
            .IsUnique(); // ห้ามซ้ำ

        builder.Property(w => w.LineName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.ModelName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Quantity)
            .IsRequired();

        builder.Property(w => w.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(w => w.UpdatedDate);

        // -------------------- Relationships --------------------
        builder.HasMany(w => w.Materials)
            .WithOne(m => m.WorkOrder)
            .HasForeignKey(m => m.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.OrderProcesses)
            .WithOne(o => o.WorkOrder)
            .HasForeignKey(o => o.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.CreatedBy)
            .WithMany()
            .HasForeignKey(w => w.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.UpdatedBy)
            .WithMany()
            .HasForeignKey(w => w.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
