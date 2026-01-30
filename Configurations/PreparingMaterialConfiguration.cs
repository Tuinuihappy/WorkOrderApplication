using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations;

public class PreparingMaterialConfiguration : IEntityTypeConfiguration<PreparingMaterial>
{
    public void Configure(EntityTypeBuilder<PreparingMaterial> builder)
    {
        // ---------------- Table ----------------
        builder.ToTable("PreparingMaterials");

        // ---------------- Primary Key ----------------
        builder.HasKey(pm => pm.Id);

        // ---------------- Properties ----------------
        builder.Property(pm => pm.PreparedQty)
               .IsRequired();

        // ---------------- Relationships ----------------

        // PreparingMaterial (N) <-> (1) PreparingProcess
        builder.HasOne(pm => pm.PreparingProcess)
               .WithMany(p => p.PreparingMaterials)
               .HasForeignKey(pm => pm.PreparingProcessId)
               .OnDelete(DeleteBehavior.Cascade);

        // PreparingMaterial (N) <-> (1) Material
        builder.HasOne(pm => pm.Material)
               .WithMany(m => m.PreparingMaterials) // ถ้า Material ไม่มี ICollection<PreparingMaterial> ให้ใช้ .WithMany()
               .HasForeignKey(pm => pm.MaterialId)
               .OnDelete(DeleteBehavior.Cascade); // ป้องกันการลบ Material ที่ถูกใช้งาน
    }
}
