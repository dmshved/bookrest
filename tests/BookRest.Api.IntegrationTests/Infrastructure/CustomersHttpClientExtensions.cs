using System.Net.Http.Json;

namespace BookRest.Api.IntegrationTests.Infrastructure;

internal static class CustomersHttpClientExtensions
{
    /// <summary>
    /// Updates the customer profile.
    /// </summary>
    /// <param name="httpClient">Default implementation of <see cref="HttpClient"/>.</param>
    /// <param name="userName">New user name.</param>
    /// <returns> A <see cref="HttpResponseMessage"/> containing HTTP response message including the status code and data.</returns>
    public static async Task<HttpResponseMessage> PutUpdateCustomerProfileAsync(this HttpClient httpClient, string? userName = null)
    {
        // Keep username unique
        userName ??= $"updated-name-{Guid.NewGuid()}";

        var response = await httpClient.PutAsJsonAsync("/api/v1/customers/me",
            new { userName });

        return response;
    }
}
