using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Interfaces;

public interface IEmailService
{
    Task<Result> SendEmailAsync(string toAddress, string subject, string body, CancellationToken cancellationToken = default);
}
