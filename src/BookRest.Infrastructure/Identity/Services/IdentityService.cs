using BookRest.Application.Common.Errors;
using BookRest.Application.Common.Interfaces;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Shared;
using BookRest.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace BookRest.Infrastructure.Identity.Services;

internal class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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

    public async Task<ApplicationUserInfo?> GetByIdAsync(Guid applicationUserId)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

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

    public async Task<Result> AddToRoleAsync(Guid applicationUserId, string role)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        if (user == null)
        {
            return Result.Failure(IdentityErrors.NotFound(applicationUserId.ToString()));
        }

        var result = await _userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
        {
            return Result.Failure(IdentityErrors.FailedAddToRole(role));
        }

        return Result.Success();
    }

    public async Task<bool> CheckPasswordAsync(Guid applicationUserId, string password)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        return user != null && await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(Guid applicationUserId)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        if (user == null)
        {
            return Result<string>.Failure(IdentityErrors.NotFound(applicationUserId.ToString()));
        }

        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        return Result<string>.Success(confirmationToken);
    }

    public async Task<Result> ConfirmEmailAsync(Guid applicationUserId, string confirmationToken)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        if (user == null)
        {
            return Result.Failure(IdentityErrors.NotFound(applicationUserId.ToString()));
        }

        if (user.EmailConfirmed)
        {
            return Result.Success();
        }

        var result = await _userManager.ConfirmEmailAsync(user, confirmationToken);

        return result.ToApplicationResult();
    }

    public async Task<bool> IsEmailConfirmedAsync(Guid applicationUserId)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        return user != null && user.EmailConfirmed;
    }

    public async Task<Result<string>> GeneratePasswordResetTokenAsync(Guid applicationUserId)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        if (user == null)
        {
            return Result<string>.Failure(IdentityErrors.NotFound(applicationUserId.ToString()));
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        return Result<string>.Success(resetToken);
    }

    public async Task<Result> ResetPasswordAsync(Guid applicationUserId, string resetToken, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        if (user == null)
        {
            return Result.Failure(IdentityErrors.NotFound(applicationUserId.ToString()));
        }

        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

        return result.ToApplicationResult();
    }

    public async Task<Result<Guid>> CreateUserAsync(string userName, string email, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);

        return result.ToApplicationResult(user.Id);
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

    public async Task<Result> SetUserNameAsync(Guid applicationUserId, string userName)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        if (user == null)
        {
            return Result<string>.Failure(IdentityErrors.NotFound(applicationUserId.ToString()));
        }

        var result = await _userManager.SetUserNameAsync(user, userName);

        return result.ToApplicationResult();
    }
}
