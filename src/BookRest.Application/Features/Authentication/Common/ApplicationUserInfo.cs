namespace BookRest.Application.Features.Authentication.Common;

public record ApplicationUserInfo(
    Guid ApplicationUserId,
    string UserName,
    string Email);
