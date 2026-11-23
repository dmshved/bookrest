namespace BookRest.Application.Common.Interfaces.Emailing;

public interface IPasswordResetLinkFactory
{
    string Create(Guid applicationUserId, string resetToken);
}
