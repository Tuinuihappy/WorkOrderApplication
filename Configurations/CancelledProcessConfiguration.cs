using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations
{
    public class CancelledProcessConfiguration : IEntityTypeConfiguration<CancelledProcess>
    {
        public void Configure(EntityTypeBuilder<CancelledProcess> builder)
        {
            // -------------------- Table Name --------------------
            builder.ToTable("CancelledProcesses");

            // -------------------- Primary Key --------------------
            builder.HasKey(c => c.Id);

            // -------------------- Properties --------------------
            builder.Property(c => c.Reason)
                .IsRequired()
                .HasMaxLength(200); // จำกัดความยาว Reason

            builder.Property(c => c.CancelledDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // ✅ ให้ DB เซ็ตเวลาเอง (SQLite / PostgreSQL)

            // -------------------- Relationships --------------------

            // (1) CancelledProcess ↔ User (CancelledBy)
            builder.HasOne(c => c.CancelledBy)
                .WithMany() // ถ้า User ไม่มี navigation กลับ
                .HasForeignKey(c => c.CancelledByUserId)
                .OnDelete(DeleteBehavior.SetNull); // ✅ ถ้า User ถูกลบ ให้เป็น null

            // (2) CancelledProcess ↔ OrderProcess (1:1)
            builder.HasOne(c => c.OrderProcess)
                .WithOne(op => op.CancelledProcess)
                .HasForeignKey<CancelledProcess>(c => c.OrderProcessId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ ถ้า OrderProcess ถูกลบ ให้ลบ CancelledProcess ด้วย

            // -------------------- Indexes --------------------
            builder.HasIndex(c => c.OrderProcessId)
                .IsUnique(); // 1 OrderProcess มี CancelledProcess เดียว
        }
    }
}
