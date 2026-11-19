using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Interfaces.Emailing;

public interface IEmailService
{
    Task<Result> SendEmailAsync(string toAddress, string subject, string body, CancellationToken cancellationToken = default);
}
