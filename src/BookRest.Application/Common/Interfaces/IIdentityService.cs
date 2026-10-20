using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(Guid applicationUserId);
    Task<string?> GetUserEmailAsync(Guid applicationUserId);
    Task<ApplicationUserInfo?> GetByEmailAsync(string email);
    Task<IEnumerable<string>> GetRolesAsync(Guid applicationUserId);

    Task<bool> IsInRoleAsync(Guid applicationUserId, string role);
    Task<bool> AuthorizeAsync(Guid applicationUserId, string policyName);
    Task<bool> CheckPasswordAsync(Guid applicationUserId, string policyName);

    Task<Result<Guid>> CreateUserAsync(string userName, string email, string password);
    Task<Result> DeleteUserAsync(Guid applicationUserId);
}
