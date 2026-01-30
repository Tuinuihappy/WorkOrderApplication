using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // ชื่อตาราง
        builder.ToTable("Users");

        // Primary Key
        builder.HasKey(u => u.Id);

        // UserName
        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100);

        // EmployeeId → Unique
        builder.Property(u => u.EmployeeId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.EmployeeId)
            .IsUnique();

        // Position
        builder.Property(u => u.Position)
            .IsRequired()
            .HasMaxLength(50);

        // Department
        builder.Property(u => u.Department)
            .IsRequired()
            .HasMaxLength(100);

        // Shift
        builder.Property(u => u.Shift)
            .IsRequired()
            .HasMaxLength(20);

        // ContactNumber (nullable)
        builder.Property(u => u.ContactNumber)
            .HasMaxLength(20);

        // Email → Unique
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        // CreatedDate / UpdatedDate
        builder.Property(u => u.CreatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
