using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations
{
    public class OrderRecordConfiguration : IEntityTypeConfiguration<OrderRecord>
    {
        public void Configure(EntityTypeBuilder<OrderRecord> builder)
        {
            // 🗃 ตั้งชื่อ Table
            builder.ToTable("OrderRecords");

            // 🔑 ใช้ Id เป็น Primary Key (จากระบบภายนอก)
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                   .ValueGeneratedNever(); // ❌ ห้าม DB auto-generate Id (ใช้ค่าจาก API)

            // 🧱 OrderName
            builder.Property(t => t.OrderName)
                   .HasMaxLength(100);

            // 🧭 LastStatus
            builder.Property(t => t.LastStatus)
                   .HasMaxLength(50)
                   .HasDefaultValue("Pending");

            // ⚙️ ExecutingIndex
            builder.Property(t => t.ExecutingIndex)
                   .IsRequired();

            // 📊 Progress
            builder.Property(t => t.Progress)
                   .HasDefaultValue(0);

            // ⏱ LastUpdated
            builder.Property(t => t.LastUpdated)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
                   .IsRequired();

            // 🧾 RawResponse → เก็บ JSON เต็มจาก API
            builder.Property(t => t.RawResponse)
                   .HasColumnType("jsonb");

            // 🏷 Source, UpdatedBy
            builder.Property(t => t.Source)
                   .HasMaxLength(50);

            builder.Property(t => t.UpdatedBy)
                   .HasMaxLength(50);
        }
    }
}
