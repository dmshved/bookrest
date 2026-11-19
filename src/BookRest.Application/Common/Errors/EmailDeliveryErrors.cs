using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Errors;

/// <summary>
/// Represents application email delivery errors.
/// </summary>
public static class EmailDeliveryErrors
{
    public static Error SendFailed() =>
        new("EmailDelivery.SendFailed",
            "Failed to send an email.",
            ErrorType.Failure);
}
