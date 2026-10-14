using BookRest.Domain.Entities;
using BookRest.Domain.ValueObjects;
using BookRest.Infrastructure.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookRest.Infrastructure.Data.Configurations;

public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.ToTable("Owners");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.UserName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(o => o.ApplicationUserId)
            .IsRequired();

        builder.HasIndex(o => o.Email)
            .IsUnique();

        builder.HasIndex(o => o.ApplicationUserId)
            .IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(o => o.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
