using BookRest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookRest.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(r => r.Token)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(r => r.Token)
            .IsUnique(true);
    }
}