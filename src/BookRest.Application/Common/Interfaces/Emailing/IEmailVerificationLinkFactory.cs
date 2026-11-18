namespace BookRest.Application.Common.Interfaces.Emailing;

public interface IEmailVerificationLinkFactory
{
    string Create(Guid applicationUserId, string confirmationToken);
}
