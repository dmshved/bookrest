namespace BookRest.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? ApplicationUserId { get; }
    bool IsAuthenticated { get; }
    IReadOnlyCollection<string> Roles { get; }
}
