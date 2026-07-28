using System.Security.Claims;
using BookRest.Application.Common.Interfaces;

namespace BookRest.Api.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextaccessor)
    {
        _httpContextAccessor = httpContextaccessor;
    }

    public string? Id => _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public List<string>? Roles => _httpContextAccessor?.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();
}