using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations
{
    public class ShipmentProcessConfiguration : IEntityTypeConfiguration<ShipmentProcess>
    {
              public void Configure(EntityTypeBuilder<ShipmentProcess> builder)
              {
                     // ---------------- Table ----------------
                     builder.ToTable("ShipmentProcesses");

                     // ---------------- Primary Key ----------------
                    builder.HasKey(s => s.Id);

                    // ---------------- Indexes ----------------
                    builder.HasIndex(s => s.SourceStation); // ✅ Index for SourceStation
                    builder.HasIndex(s => s.DestinationStation); // ✅ Index for DestinationStation
                     

                    builder.Property(rp => rp.ArrivalTime)
                        .IsRequired(false);

                     // ---------------- Relationships ----------------

                     // ShipmentProcess (1) <-> (1) OrderProcess
                     builder.HasOne(s => s.OrderProcess)
                            .WithOne(o => o.ShipmentProcess)   // 1 OrderProcess มี 1 ShipmentProcess
                            .HasForeignKey<ShipmentProcess>(s => s.OrderProcessId)
                            .OnDelete(DeleteBehavior.Cascade);

                   
        }
    }
}
