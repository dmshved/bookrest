using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Models;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BookRest.Infrastructure.Identity.Services;

internal class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string?> GetUserNameAsync(Guid applicationUserId)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        return user?.UserName;
    }

    public async Task<string?> GetUserEmailAsync(Guid applicationUserId)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        return user?.Email;
    }

    public async Task<ApplicationUserInfo?> GetByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        return user != null ? new ApplicationUserInfo(user.Id, user.UserName!, user.Email!) : null;
    }

    public async Task<IEnumerable<string>> GetRolesAsync(Guid applicationUserId)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        return user != null ? await _userManager.GetRolesAsync(user) : Enumerable.Empty<string>();
    }

    public async Task<bool> IsInRoleAsync(Guid applicationUserId, string role)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(Guid applicationUserId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<bool> CheckPasswordAsync(Guid applicationUserId, string password)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        return user != null && await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<(Result, Guid applicationUserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<Result> DeleteUserAsync(Guid applicationUserId)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }
}
