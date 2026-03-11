using Microsoft.AspNetCore.Identity;

namespace BookRest.Api.Models;

public sealed class ApplicationUser : IdentityUser
{
    public bool EnableNotifications { get; set; }
    public string Initials { get; set; } = string.Empty;
}