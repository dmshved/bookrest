namespace BookRest.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? ApplicationUserId { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string>? Roles { get; }
}
