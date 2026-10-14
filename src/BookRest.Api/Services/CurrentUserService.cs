using System.Security.Claims;
using BookRest.Application.Common.Interfaces;

namespace BookRest.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextaccessor)
    {
        _httpContextAccessor = httpContextaccessor;
    }

    public Guid? ApplicationUserId
    {
        get
        {
            var id = _httpContextAccessor?
                .HttpContext?
                .User?
                .FindFirstValue("application_user_id");

            return Guid.TryParse(id, out var applicationUserId)
                ? applicationUserId
                : null;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor?
            .HttpContext?
            .User?
            .Identity?
            .IsAuthenticated ?? false;

    public IEnumerable<string>? Roles =>
        _httpContextAccessor?
            .HttpContext?
            .User?
            .FindAll("role")
            .Select(x => x.Value)
            .ToList();
}
