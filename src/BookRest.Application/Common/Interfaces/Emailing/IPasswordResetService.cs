using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Interfaces.Emailing;

public interface IPasswordResetService
{
    Task<Result> SendAsync(Guid applicationUserId, string toAddress, CancellationToken cancellationToken = default);
}
