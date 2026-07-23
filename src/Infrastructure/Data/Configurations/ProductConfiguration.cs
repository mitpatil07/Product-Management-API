using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProductName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CreatedOn)
            .IsRequired();

        builder.Property(p => p.ModifiedBy)
            .HasMaxLength(100);

        builder.HasIndex(p => p.ProductName);
    }
}
