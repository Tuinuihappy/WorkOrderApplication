using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations;

public class PreparingProcessConfiguration : IEntityTypeConfiguration<PreparingProcess>
{
    public void Configure(EntityTypeBuilder<PreparingProcess> builder)
    {
        // ---------------- Table ----------------
        builder.ToTable("PreparingProcesses");

        // ---------------- Primary Key ----------------
        builder.HasKey(p => p.Id);

        // ---------------- Properties ----------------
        builder.Property(p => p.PreparedDate)
               .IsRequired()
               .HasDefaultValueSql("CURRENT_TIMESTAMP"); 
               // ✅ DB กำหนดเวลาเองถ้าไม่ส่งมา
               

        // ---------------- Relationships ----------------
        
        // PreparingProcess (N) <-> (1) User (PreparingBy)
        builder.HasOne(p => p.PreparingBy)
               .WithMany() // User ไม่ได้เก็บ Collection ของ PreparingProcess
               .HasForeignKey(p => p.PreparingByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        // PreparingProcess (1) <-> (1) OrderProcess
        builder.HasOne(p => p.OrderProcess)
               .WithOne(o => o.PreparingProcess) // 1 OrderProcess มี 1 PreparingProcess
               .HasForeignKey<PreparingProcess>(p => p.OrderProcessId)
               .OnDelete(DeleteBehavior.Cascade);

        // PreparingProcess (1) <-> (N) PreparingMaterial
        builder.HasMany(p => p.PreparingMaterials)
               .WithOne(pm => pm.PreparingProcess)
               .HasForeignKey(pm => pm.PreparingProcessId)
               .OnDelete(DeleteBehavior.Cascade);
               
    }
}
