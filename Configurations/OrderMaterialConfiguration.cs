using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderApplication.API.Entities;

namespace WorkOrderApplication.API.Configurations;

public class OrderMaterialConfiguration : IEntityTypeConfiguration<OrderMaterial>
{
    public void Configure(EntityTypeBuilder<OrderMaterial> builder)
    {
        builder.ToTable("OrderMaterials");

        builder.HasKey(om => om.Id);

        builder.Property(om => om.OrderQty)
               .IsRequired();

        builder.HasOne(om => om.Material)
               .WithMany(m => m.OrderMaterials)
               .HasForeignKey(om => om.MaterialId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(om => om.OrderProcess)
               .WithMany(op => op.OrderMaterials)
               .HasForeignKey(om => om.OrderProcessId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
