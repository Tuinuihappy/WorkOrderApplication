using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations;

public class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        // -------------------- Table --------------------
        builder.ToTable("Materials");

        // -------------------- Primary Key --------------------
        builder.HasKey(m => m.Id);

        // -------------------- Properties --------------------
        builder.Property(m => m.MaterialNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.MaterialDescription)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.ReqmntQty)
            .IsRequired();

        builder.Property(m => m.QtyWthdrn)
            .IsRequired();

        builder.Property(m => m.BUn)
            .IsRequired()
            .HasMaxLength(50);

        // -------------------- Index --------------------
        // MaterialNumber สามารถซ้ำกันได้ แต่ถ้าต้องการให้ unique ต่อ WorkOrderId
        builder.HasIndex(m => new { m.WorkOrderId, m.MaterialNumber })
            .IsUnique();

        // -------------------- Relationships --------------------
        builder.HasOne(m => m.WorkOrder)
            .WithMany(w => w.Materials)
            .HasForeignKey(m => m.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.OrderMaterials)
            .WithOne(om => om.Material)
            .HasForeignKey(om => om.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.PreparingMaterials)
            .WithOne(pm => pm.Material)
            .HasForeignKey(pm => pm.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.ReceivedMaterials)
            .WithOne(rm => rm.Material)
            .HasForeignKey(rm => rm.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
