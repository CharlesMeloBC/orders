using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Products;

namespace ECommerce.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("createdAt")
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updatedAt")
            .IsRequired();

        builder.Property(x => x.DeletedAtUtc)
            .HasColumnName("deletedAt");
    }
}
