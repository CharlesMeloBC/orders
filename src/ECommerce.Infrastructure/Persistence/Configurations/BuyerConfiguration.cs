using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Buyers;

namespace ECommerce.Infrastructure.Persistence.Configurations;

internal sealed class BuyerConfiguration : IEntityTypeConfiguration<Buyer>
{
    public void Configure(EntityTypeBuilder<Buyer> builder)
    {
        builder.ToTable("Buyers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
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
