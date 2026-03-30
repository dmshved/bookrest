using BookRest.Application.Common.Models;

namespace BookRest.Application.Common.Interfaces;

public interface IIdentityService 
{
    Task<string>? GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result, string userId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);
}
