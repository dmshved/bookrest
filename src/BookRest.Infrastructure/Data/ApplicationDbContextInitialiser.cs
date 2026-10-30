// See Database Initialisation Strategies at https://jasontaylor.dev/ef-core-database-initialisation-strategies/
// See Applying Migrations at https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli
// Currently Eearly Development strategy

using System.Security.Claims;
using BookRest.Domain.Constants;
using BookRest.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookRest.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ApplicationDbContextInitialiser(
            ILogger<ApplicationDbContextInitialiser> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        foreach (var role in Roles.All())
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        // Default users
        var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };

        if (await _userManager.FindByNameAsync(administrator.UserName) == null)
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(Roles.Administrator))
            {
                await _userManager.AddToRolesAsync(administrator, new[] { Roles.Administrator });
            }
        }

        // Administrator permissions
        await GrantAsync(Roles.Administrator, Permissions.All);

        // Owner permissions
        await GrantAsync(Roles.Owner,
        [
            Permissions.Restaurants.View,
            Permissions.Restaurants.Create,
            Permissions.Restaurants.Edit,
            Permissions.Orders.View,
            Permissions.Orders.Edit
        ]);

        // Customer permissions
        await GrantAsync(Roles.Customer,
        [
            Permissions.Restaurants.View,
            Permissions.Orders.View,
            Permissions.Orders.Create
        ]);
    }

    private async Task GrantAsync(string roleName, IEnumerable<string> permissions)
    {
        var role = await _roleManager.FindByNameAsync(roleName);

        if (role == null)
        {
            throw new InvalidOperationException($"Role '{roleName}' was not found.");
        }

        var existingClaims = await _roleManager.GetClaimsAsync(role);

        foreach (var permission in permissions)
        {
            if (existingClaims.Any(claim =>
                    claim.Type == Permissions.ClaimType &&
                    claim.Value == permission))
            {
                // Skip if permission is already persisted
                continue;
            }

            var result = await _roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Cannot grant permission '{permission}' to the role '{roleName}'.");
            }
        }
    }
}
