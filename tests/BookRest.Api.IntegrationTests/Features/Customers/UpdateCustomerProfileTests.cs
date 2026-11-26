using System.Net;
using System.Net.Http.Json;
using BookRest.Api.IntegrationTests.Infrastructure;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Constants;
using Microsoft.AspNetCore.Mvc;

namespace BookRest.Api.IntegrationTests.Features.Customers;

[Collection(IntegrationTestCollection.Name)]
public class UpdateCustomerProfileTests
{
    private readonly BookRestWebApplicationFactory _factory;

    public UpdateCustomerProfileTests(BookRestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Registers a new <see cref="HttpClient"/> client, logs into the system to obtain an access token, and creates a new <see cref="HttpClient"/> client using that access token.
    /// </summary>
    /// <returns>An authorized <see cref="HttpClient"/> client, <see cref="AuthenticationResponseDto"/> details, and the <see cref="string"/> email address of the newly created user.</returns>
    private async Task<(HttpClient HttpClient, AuthenticationResponseDto AuthResponse, string Email)> CreateAuthenticatedCustomerClientAsync()
    {
        // Create unauthenticated http client
        var httpClient = _factory.CreateClient();
        // Register customer
        var email = await httpClient.RegisterCustomerAsync();
        // Get authentication data
        var authResponse = await httpClient.LoginAsync(email);
        // Create authenticated http client
        var authenticatedHttpClient = _factory.CreateClientWithBearerToken(authResponse.AccessToken);

        return (authenticatedHttpClient, authResponse, email);
    }

    /// <summary>
    /// Performs a login for an existing user and creates an authorized <see cref="HttpClient"/> client using his access token.
    /// </summary>
    /// <param name="email">User email.</param>
    /// <returns>An authorized <see cref="HttpClient"/> client.</returns>
    private async Task<HttpClient> CreateAuthenticatedCustomerClientForAsync(string email)
    {
        // Create unauthenticated http client
        var httpClient = _factory.CreateClient();
        // Get authentication data
        var authResponse = await httpClient.LoginAsync(email);
        // Create authenticated http client
        var authenticatedHttpClient = _factory.CreateClientWithBearerToken(authResponse.AccessToken);

        return authenticatedHttpClient;
    }

    [Fact]
    public async Task UpdateCustomerProfile_WhenValidRequest_ShouldUpdateCustomerProfileAndIdentityUserAndReturnOk()
    {
        var (httpClient, authResponse, _) = await CreateAuthenticatedCustomerClientAsync();
        var newName = $"updated-{Guid.NewGuid()}";

        var updateCustomerProfileResponse = await httpClient.PutUpdateCustomerProfileAsync(newName);

        Assert.Equal(HttpStatusCode.OK, updateCustomerProfileResponse.StatusCode);
        var updatedCustomer = await _factory.GetStoredCustomerByApplicationUserIdAsync(authResponse.ApplicationUserId);
        Assert.Equal(newName, updatedCustomer!.UserName);
        Assert.Equal(newName, await _factory.GetStoredApplicationUserNameAsync(authResponse.ApplicationUserId));
    }

    [Fact]
    public async Task UpdateCustomerProfile_WhenNameIsTheSame_ShouldReturnOkAndUpdateNothing()
    {
        var (httpClient, authResponse, _) = await CreateAuthenticatedCustomerClientAsync();
        var currentName = (await _factory.GetStoredCustomerByApplicationUserIdAsync(authResponse.ApplicationUserId))!.UserName;

        var updateCustomerProfileResponse = await httpClient.PutUpdateCustomerProfileAsync(currentName);

        Assert.Equal(HttpStatusCode.OK, updateCustomerProfileResponse.StatusCode);
        Assert.Equal(currentName, (await _factory.GetStoredCustomerByApplicationUserIdAsync(authResponse.ApplicationUserId))!.UserName);
    }

    [Fact]
    public async Task UpdateCustomerProfile_WhenUserNameIsAlreadyTaken_ShouldReturn400BadRequest()
    {
        // Register customer with predefined name
        var takenName = $"taken-{Guid.NewGuid()}";
        await _factory.CreateClient().PostRegisterCustomerAsync(userName: takenName);
        // Register another customer
        var (httpClient, authResponse, _) = await CreateAuthenticatedCustomerClientAsync();
        var originalName = (await _factory.GetStoredCustomerByApplicationUserIdAsync(authResponse.ApplicationUserId))!.UserName;

        var updateCustomerProfileResponse = await httpClient.PutUpdateCustomerProfileAsync(takenName);

        Assert.Equal(HttpStatusCode.BadRequest, updateCustomerProfileResponse.StatusCode);
        var problemDetails = await updateCustomerProfileResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("DuplicateUserName", problemDetails!.Errors.Keys);
    }

    [Fact]
    public async Task UpdateCustomerProfile_WhenUserNameWasChanged_ShouldStillAllowLogin()
    {
        var (httpClient, _, email) = await CreateAuthenticatedCustomerClientAsync();

        await httpClient.PutUpdateCustomerProfileAsync();

        var loginResponse = await httpClient.PostLoginAsync(email);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomerProfile_WhenAccessTokenNotProvided_ShouldReturn401Unauthorized()
    {
        var updateCustomerProfileResponse = await _factory.CreateClient().PutUpdateCustomerProfileAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, updateCustomerProfileResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomerProfile_WhenUserIsOwner_ShouldReturn403Forbidden()
    {
        var email = await _factory.CreateClient().RegisterOwnerAsync();
        using var httpClient = await CreateAuthenticatedCustomerClientForAsync(email);

        var updateCustomerProfileResponse = await httpClient.PutUpdateCustomerProfileAsync();

        Assert.Equal(HttpStatusCode.Forbidden, updateCustomerProfileResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomerProfile_WhenUserHasNoPermissions_ShouldReturn403Forbidden()
    {
        // Create a user with no permissions
        var email = await _factory.CreateUserWithPermissionsAsync();
        using var httpClient = await CreateAuthenticatedCustomerClientForAsync(email);

        var updateCustomerProfileResponse = await httpClient.PutUpdateCustomerProfileAsync();

        Assert.Equal(HttpStatusCode.Forbidden, updateCustomerProfileResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomerProfile_WhenUserHasPermissionButNoCustomerProfile_ShouldReturn404NotFound()
    {
        // Create a user with no permissions
        var email = await _factory.CreateUserWithPermissionsAsync(Permissions.Customers.Edit);
        using var httpClient = await CreateAuthenticatedCustomerClientForAsync(email);

        var updateCustomerProfileResponse = await httpClient.PutUpdateCustomerProfileAsync();

        Assert.Equal(HttpStatusCode.NotFound, updateCustomerProfileResponse.StatusCode);
    }

    [Theory]
    [InlineData("")] // User name is empty
    [InlineData("abc")] // User name is too short
    public async Task UpdateCustomerProfile_WhenUserNameIsInvalid_ShouldReturn400BadRequest(string userName)
    {
        var (httpClient, _, _) = await CreateAuthenticatedCustomerClientAsync();

        var updateCustomerProfileResponse = await httpClient.PutUpdateCustomerProfileAsync(userName);

        Assert.Equal(HttpStatusCode.BadRequest, updateCustomerProfileResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomerProfile_WhenUserNameIsTooLong_ShouldReturn400BadRequest()
    {
        var tooLongName = new string('x', 51);

        var (httpClient, _, _) = await CreateAuthenticatedCustomerClientAsync();

        var updateCustomerProfileResponse = await httpClient.PutUpdateCustomerProfileAsync(tooLongName);

        Assert.Equal(HttpStatusCode.BadRequest, updateCustomerProfileResponse.StatusCode);
    }
}
