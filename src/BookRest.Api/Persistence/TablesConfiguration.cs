using BookRest.Api.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BookRest.Api.Persistence;

public static class TablesConfiguration
{
    public static void ConfigureTables(this ModelBuilder modelBuilder)
    {
        IdentityTablesConfiguration.ConfigureIdentityTables(modelBuilder);
    }
}
