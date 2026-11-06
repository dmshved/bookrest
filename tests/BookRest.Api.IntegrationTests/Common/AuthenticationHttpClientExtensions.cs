using System.Net.Http.Json;
using BookRest.Application.Features.Authentication.Common;

namespace BookRest.Api.IntegrationTests.Common;

internal static class AuthenticationHttpClientExtensions
{
    public const string DefaultPassword = "Password1!";

    public static async Task<AuthenticationResponseDto> RegisterCustomerAsync(this HttpClient httpClient, string password = DefaultPassword)
    {
        // Keep email unique
        var email = $"{Guid.NewGuid()}@gmail.com";

        var registerResponse = await httpClient.PostAsJsonAsync("/api/v1/auth/customers/register",
            new
            {
                userName = "tester",
                email,
                password,
                confirmPassword = password
            });

        registerResponse.EnsureSuccessStatusCode();

        // Auto login
        return await httpClient.LoginAsync(email, password);
    }

    public static async Task<AuthenticationResponseDto> LoginAsync(this HttpClient httpClient, string email, string password = DefaultPassword)
    {
        var loginResponse = await httpClient.PostAsJsonAsync("/api/v1/auth/login",
            new
            {
                email,
                password
            });

        loginResponse.EnsureSuccessStatusCode();

        var result = await loginResponse.Content.ReadFromJsonAsync<AuthenticationResponseDto>();

        return result!;
    }
}
