using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations;

public class ReceivedProcessConfiguration : IEntityTypeConfiguration<ReceivedProcess>
{
    public void Configure(EntityTypeBuilder<ReceivedProcess> builder)
    {
        builder.ToTable("ReceivedProcesses");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.ReceivedDate)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // User (N:1)
        builder.HasOne(rp => rp.ReceivedBy)
            .WithMany()
            .HasForeignKey(rp => rp.ReceivedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Property(rm => rm.ShortageReason)
            .HasMaxLength(255)
            .IsRequired(false);        
            
        // OrderProcess (1:1)
        builder.HasOne(rp => rp.OrderProcess)
            .WithOne(op => op.ReceiveProcess)
            .HasForeignKey<ReceivedProcess>(rp => rp.OrderProcessId)
            .OnDelete(DeleteBehavior.Cascade);

        // ReceivedMaterials (1:M)
        builder.HasMany(rp => rp.ReceivedMaterials)
            .WithOne(rm => rm.ReceivedProcess)
            .HasForeignKey(rm => rm.ReceivedProcessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
