using BookRest.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookRest.Api.Data.Configurations;

public static class IdentityTablesConfiguration
{
    public static void ConfigureIdentityTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().ToTable("AspNetUsers", "identity");
        modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles", "identity");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles", "identity");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims", "identity");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins", "identity");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims", "identity");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens", "identity");
    }
}
