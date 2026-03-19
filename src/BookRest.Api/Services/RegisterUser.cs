// !!! ATTENTION !!!
// Currently at the lines 34 and 41 I'm loosing useful Identity exceptions
// Potential solution: custom Result<T> type
// example as follows
//public class Result<T>
//{
//    public bool Success { get; init; }
//    public T? Value { get; init; }
//    public List<string> Errors { get; init; } = [];
//}

using BookRest.Api.Constants;
using BookRest.Api.Data;
using BookRest.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace BookRest.Api.Features;

internal sealed class RegisterUser(AppDbContext context, UserManager<AppUser> userManager)
{
    public record Request(string Email, string Initials, string Password, bool EnableNotifications);
    public record Response(string Id, string Email);

    public async Task<Response> Handle(Request request)
    {
        // I created a separate transaction because.CreateAsync() and .AddToRoleAsync()
        // are performing separate small transactions, this way db can become inconsistent
        using var transaction = await context.Database.BeginTransactionAsync();

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            Initials = request.Initials,
            EnableNotifications = request.EnableNotifications,
        };

        // Create user
        IdentityResult identityResult = await userManager.CreateAsync(user, request.Password);
        if (identityResult.Succeeded == false)
        {
            throw new ApplicationException("User creation failed");
        }

        // Add user to the "Member" role
        IdentityResult addToRoleResult = await userManager.AddToRoleAsync(user, Roles.Member);
        if (addToRoleResult.Succeeded == false)
        {
            throw new ApplicationException("Assigning user to the role provided to error");
        }

        await transaction.CommitAsync();

        return new Response(user.Id, user.Email); // return user just for testing
    }
}
