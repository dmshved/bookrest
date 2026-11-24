using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(Guid applicationUserId);
    Task<string?> GetUserEmailAsync(Guid applicationUserId);
    Task<ApplicationUserInfo?> GetByEmailAsync(string email);
    Task<ApplicationUserInfo?> GetByIdAsync(Guid applicationUserId);
    Task<IEnumerable<string>> GetRolesAsync(Guid applicationUserId);

    Task<bool> IsInRoleAsync(Guid applicationUserId, string role);
    Task<Result> AddToRoleAsync(Guid applicationUserId, string role);
    Task<bool> CheckPasswordAsync(Guid applicationUserId, string password);

    Task<Result<string>> GenerateEmailConfirmationTokenAsync(Guid applicationUserId);
    Task<Result> ConfirmEmailAsync(Guid applicationUserId, string confirmationToken);
    Task<bool> IsEmailConfirmedAsync(Guid applicationUserId);

    Task<Result<string>> GeneratePasswordResetTokenAsync(Guid applicationUserId);
    Task<Result> ResetPasswordAsync(Guid applicationUserId, string resetToken, string newPassword);

    Task<Result<Guid>> CreateUserAsync(string userName, string email, string password);
    Task<Result> DeleteUserAsync(Guid applicationUserId);
}
