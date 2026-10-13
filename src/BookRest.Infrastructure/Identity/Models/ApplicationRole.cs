using Microsoft.AspNetCore.Identity;

namespace BookRest.Infrastructure.Identity.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base() { }

    public ApplicationRole(string roleName) : base(roleName) { }
}
