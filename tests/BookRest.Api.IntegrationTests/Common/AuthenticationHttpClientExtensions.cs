using System.Net.Http.Json;
using BookRest.Application.Features.Authentication.Common;

namespace BookRest.Api.IntegrationTests.Common;

internal static class AuthenticationHttpClientExtensions
{
    public const string DefaultPassword = "Password1!";

    /// <summary>
    /// Registers customer and returns an email.
    /// </summary>
    /// <param name="httpClient">Default implementation of <see cref="HttpClient"/>.</param>
    /// <param name="password">User password.</param>
    /// <returns> A <see cref="string"/> containing email of registered user.</returns>
    public static async Task<string> RegisterCustomerAsync(this HttpClient httpClient, string password = DefaultPassword)
    {
        // Keep email unique
        var email = $"{Guid.NewGuid()}@gmail.com";
        // Keep username unique
        var userName = $"tester-{Guid.NewGuid()}";

        var response = await httpClient.PostAsJsonAsync("/api/v1/auth/customers/register",
            new { userName, email, password, confirmPassword = password });

        response.EnsureSuccessStatusCode();

        return email;
    }

    /// <summary>
    /// Logins user and returns jwt refresh and access tokens.
    /// Returns raw <see cref="HttpResponseMessage"/> for tests that assert on failures.
    /// </summary>
    /// <param name="httpClient">Default implementation of <see cref="HttpClient"/>.</param>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <returns> A <see cref="HttpResponseMessage"/> containing HTTP response message including the status code and data.</returns>
    public static async Task<HttpResponseMessage> PostLoginAsync(this HttpClient httpClient, string email, string password = DefaultPassword)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password });

        return response;
    }

    /// <summary>
    /// Logins user and returns jwt refresh and access tokens.
    /// </summary>
    /// <param name="httpClient">Default implementation of <see cref="HttpClient"/>.</param>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <returns>An <see cref="AuthenticationResponseDto"/> containing authentication information about user.</returns>
    public static async Task<AuthenticationResponseDto> LoginAsync(this HttpClient httpClient, string email, string password = DefaultPassword)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password });

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<AuthenticationResponseDto>())!;
    }

    /// <summary>
    /// Exchanges the given JWT refresh token for a new access token and returns the updated authentication information.
    /// Returns raw <see cref="HttpResponseMessage"/> for tests that assert on failures.
    /// </summary>
    /// <param name="httpClient">Default implementation of <see cref="HttpClient"/>.</param>
    /// <param name="refreshToken">JWT refresh token.</param>
    /// <returns> A <see cref="HttpResponseMessage"/> containing HTTP response message including the status code and data.</returns>
    public static async Task<HttpResponseMessage> PostRefreshAsync(this HttpClient httpClient, string refreshToken)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/auth/refresh",
            new { refreshToken });

        return response;
    }

    /// <summary>
    /// Exchanges the given JWT refresh token for a new access token and returns the updated authentication information.
    /// </summary>
    /// <param name="httpClient">Default implementation of <see cref="HttpClient"/>.</param>
    /// <param name="refreshToken">JWT refresh token.</param>
    /// <returns>An <see cref="AuthenticationResponseDto"/> containing authentication information about user.</returns>
    public static async Task<AuthenticationResponseDto> RefreshAsync(this HttpClient httpClient, string refreshToken)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/auth/refresh",
            new { refreshToken });

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<AuthenticationResponseDto>())!;
    }

    /// <summary>
    /// Exchanges the given JWT refresh token for a new access token and returns the updated authentication information.
    /// Returns raw <see cref="HttpResponseMessage"/> for tests that assert on failures.
    /// </summary>
    /// <param name="httpClient">Default implementation of <see cref="HttpClient"/>.</param>
    /// <param name="refreshToken">JWT refresh token.</param>
    /// <param name="reason">Reason for revoking the token.</param>
    /// <returns> A <see cref="HttpResponseMessage"/> containing HTTP response message including the status code and data.</returns>
    public static async Task<HttpResponseMessage> PostRevokeAsync(this HttpClient httpClient, string refreshToken, string? reason = null)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/auth/revoke",
            new { refreshToken, reason });

        return response;
    }
}
