using Domain.Entities.Sellers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SellerConfiguration : IEntityTypeConfiguration<Seller>
{
    public void Configure(EntityTypeBuilder<Seller> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FirstName).IsRequired().HasMaxLength(100);

        builder.Property(s => s.LastName).IsRequired().HasMaxLength(100);

        builder.Property(s => s.PhoneNumber).IsRequired().HasMaxLength(50).IsUnicode(false);

        builder.Property(s => s.Email).IsRequired().HasMaxLength(255).IsUnicode(false);

        builder.Property(s => s.Region).IsRequired().HasMaxLength(50);

        builder.Property(s => s.IsActive).IsRequired();

        builder.HasIndex(s => s.Email).IsUnique();

        builder.HasIndex(s => s.CreatedAt);

        builder.HasIndex(s => new { s.Region, s.CreatedAt });

        builder.HasQueryFilter(s => s.IsActive);
    }
}