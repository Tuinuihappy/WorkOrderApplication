using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations;

public class OrderGroupAMRConfiguration : IEntityTypeConfiguration<OrderGroupAMR>
{
    public void Configure(EntityTypeBuilder<OrderGroupAMR> builder)
    {
        // 🧱 ชื่อตาราง
        builder.ToTable("OrderGroupAMRs");

        // 🔑 Primary Key
        builder.HasKey(x => x.Id);

        // 🆔 Id auto increment
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // 🧩 SourceStationId
        builder.Property(x => x.SourceStationId)
            .IsRequired();

        // 🧩 SourceStation
        builder.Property(x => x.SourceStation)
            .IsRequired()
            .HasMaxLength(100);

        // 🧩 DestinationStationId
        builder.Property(x => x.DestinationStationId)
            .IsRequired();

        // 🧩 DestinationStation
        builder.Property(x => x.DestinationStation)
            .IsRequired()
            .HasMaxLength(100);

        // 🧩 OrderGroupId
        builder.Property(x => x.OrderGroupId)
            .IsRequired();

        // ⚙️ Unique Index เพื่อป้องกัน Mapping ซ้ำ (เช่น SHELF1 → SHELF2)
        builder.HasIndex(x => new { x.SourceStationId, x.DestinationStationId })
            .IsUnique();

        // ⚡ เพิ่ม Index เพื่อให้ Query เร็วขึ้น
        builder.HasIndex(x => x.OrderGroupId);
    }
}
