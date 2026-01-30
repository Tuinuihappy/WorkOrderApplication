using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations;

public class ConfirmProcessConfiguration : IEntityTypeConfiguration<ConfirmProcess>
{
    public void Configure(EntityTypeBuilder<ConfirmProcess> builder)
    {
        // ---------------- Table ----------------
        builder.ToTable("ConfirmProcesses");

        // ---------------- Primary Key ----------------
        builder.HasKey(c => c.Id);

        // ---------------- Properties ----------------
        builder.Property(c => c.ConfirmedDate)
               .IsRequired();

        // ---------------- Relationships ----------------
        builder.HasOne(c => c.OrderProcess)
               .WithOne(o => o.ConfirmProcess) // ✅ One-to-One
               .HasForeignKey<ConfirmProcess>(c => c.OrderProcessId)
               .OnDelete(DeleteBehavior.Cascade);

    }
}
