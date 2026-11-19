using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Interfaces.Emailing;

public interface IEmailVerificationService
{
    Task<Result> SendAsync(Guid applicationUserId, string toAddress, CancellationToken cancellationToken = default);
}
