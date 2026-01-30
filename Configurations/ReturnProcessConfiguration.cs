using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations;

public class ReturnProcessConfiguration : IEntityTypeConfiguration<ReturnProcess>
{
    public void Configure(EntityTypeBuilder<ReturnProcess> builder)
    {
        builder.ToTable("ReturnProcesses");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.ReturnDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(rp => rp.Reason)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne(rp => rp.ReturnByUser)
            .WithMany()
            .HasForeignKey(rp => rp.ReturnByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rp => rp.OrderProcess)
            .WithOne(op => op.ReturnProcess)
            .HasForeignKey<ReturnProcess>(rp => rp.OrderProcessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
