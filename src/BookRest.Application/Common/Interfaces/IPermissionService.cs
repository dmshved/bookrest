using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid applicationUserId, CancellationToken cancellationToken = default);
    Task<Result> GrantPermissionAsync(string roleName, string permission, CancellationToken cancellationToken = default);
    Task<Result> RevokePermissionAsync(string roleName, string permission, CancellationToken cancellationToken = default);
}
