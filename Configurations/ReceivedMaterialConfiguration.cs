using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations;

public class ReceivedMaterialConfiguration : IEntityTypeConfiguration<ReceivedMaterial>
{
    public void Configure(EntityTypeBuilder<ReceivedMaterial> builder)
    {
        builder.ToTable("ReceivedMaterials");

        // -------------------- Primary Key --------------------
        builder.HasKey(rm => rm.Id);

        // -------------------- Properties --------------------
        builder.Property(rm => rm.ReceivedQty)
            .IsRequired()
            .HasDefaultValue(0);

        // -------------------- Relationships --------------------
        // ReceivedProcess (1) -> (Many) ReceivedMaterials
        builder.HasOne(rm => rm.ReceivedProcess)
            .WithMany(rp => rp.ReceivedMaterials)
            .HasForeignKey(rm => rm.ReceivedProcessId)
            .OnDelete(DeleteBehavior.Cascade);

        // Material (1) -> (Many) ReceivedMaterials
        builder.HasOne(rm => rm.Material)
            .WithMany(m => m.ReceivedMaterials)   // ✅ อ้างถึง navigation property ที่อยู่ใน Material
            .HasForeignKey(rm => rm.MaterialId)  // ✅ ใช้ FK ชัดเจน
            .OnDelete(DeleteBehavior.Cascade);   // ปกติ ReceivedMaterial ผูกกับ Material → ถ้า Material หาย ให้ลบตาม
    }
}
