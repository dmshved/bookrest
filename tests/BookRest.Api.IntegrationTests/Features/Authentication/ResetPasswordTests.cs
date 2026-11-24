using System.Net;
using System.Net.Http.Json;
using BookRest.Api.IntegrationTests.Common.Models.Emailing;
using BookRest.Api.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BookRest.Api.IntegrationTests.Features.Authentication;

[Collection(IntegrationTestCollection.Name)]
public class ResetPasswordTests
{
    private readonly BookRestWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public ResetPasswordTests(BookRestWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    /// <summary>
    /// Registers customer and sends the forgot password requestt.
    /// </summary>
    /// <returns>Returns an email and <see cref="PasswordResetLink"/> of registered customer.</returns>
    private async Task<(string Email, PasswordResetLink passwordResetLink)> RequestForgotPasswordAsync()
    {
        var email = await _httpClient.RegisterCustomerAsync();

        await _httpClient.PostForgotPasswordAsync(email);

        var passwordResetLink = _factory.PasswordResetEmailsSentTo(email)
            .Last()
            .GetPasswordResetLink();

        return (email, passwordResetLink);
    }

    [Fact]
    public async Task ResetPassword_WhenResetTokenFromEmailIsValid_ShouldChangePassword()
    {
        var (email, resetLink) = await RequestForgotPasswordAsync();
        var newPassword = "NewPassword1!";

        var resetPasswordResponse = await _httpClient.PostResetPasswordAsync(resetLink.ApplicationUserId, resetLink.ResetToken, newPassword);
        Assert.Equal(HttpStatusCode.OK, resetPasswordResponse.StatusCode);

        // Login with new password should be successfull
        var firstLoginResponse = await _httpClient.PostLoginAsync(email, newPassword);
        Assert.Equal(HttpStatusCode.OK, firstLoginResponse.StatusCode);
        // Login with default password should be failure
        var secondLoginResponse = await _httpClient.PostLoginAsync(email);
        Assert.Equal(HttpStatusCode.Unauthorized, secondLoginResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenResetTokenUsedTwice_ShouldReturn400BadRequest()
    {
        // Register customer and send password reset request
        var (_, resetLink) = await RequestForgotPasswordAsync();

        // Use password reset token twice
        var firstPasswordResetResponse = await _httpClient.PostResetPasswordAsync(resetLink.ApplicationUserId, resetLink.ResetToken);
        var secondPasswordResetResponse = await _httpClient.PostResetPasswordAsync(resetLink.ApplicationUserId, resetLink.ResetToken);

        Assert.Equal(HttpStatusCode.OK, firstPasswordResetResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, secondPasswordResetResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenTokenIsCorrupted_ShouldReturn400AndKeepOldPassword()
    {
        var (email, resetLink) = await RequestForgotPasswordAsync();
        // Corrupt the password reset token
        var corruptedToken = resetLink.ResetToken + "x";

        // Send password reset request with corrupted token
        var resetPasswordResponse = await _httpClient.PostResetPasswordAsync(resetLink.ApplicationUserId, corruptedToken);

        Assert.Equal(HttpStatusCode.BadRequest, resetPasswordResponse.StatusCode);

        // Check that user still can login with old password
        var loginResponse = await _httpClient.PostLoginAsync(email);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenResetTokenBelongsToAnotherUser_ShouldReturn400BadRequest()
    {
        // Register two customers and get their password rest links
        var (firstEmail, firstResetLink) = await RequestForgotPasswordAsync();
        var (secondEmail, secondResetLink) = await RequestForgotPasswordAsync();

        // Send password reset request with first user id and second user reset token
        var resetPasswordResponse = await _httpClient.PostResetPasswordAsync(firstResetLink.ApplicationUserId, secondResetLink.ResetToken);
        Assert.Equal(HttpStatusCode.BadRequest, resetPasswordResponse.StatusCode);

        // Check that first user still can login with old password
        var loginResponse = await _httpClient.PostLoginAsync(firstEmail);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenNewPasswordViolatesIdentityRules_ShouldReturn400BadRequestAndKeepOldPassword()
    {
        var (email, resetLink) = await RequestForgotPasswordAsync();
        // Set invalid password
        var invalidPassword = "invalid-password";

        // Reset password with invalid password
        var resetPasswordResponse = await _httpClient.PostResetPasswordAsync(resetLink.ApplicationUserId, resetLink.ResetToken, invalidPassword);
        Assert.Equal(HttpStatusCode.BadRequest, resetPasswordResponse.StatusCode);
        var problemDetails = await resetPasswordResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("PasswordRequiresDigit", problemDetails!.Errors.Keys);

        // Check that second user still can login with old password
        var loginResponse = await _httpClient.PostLoginAsync(email);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenFirstAttemptWasUnsuccessful_ShouldAllowRetry()
    {
        var (email, resetLink) = await RequestForgotPasswordAsync();
        // Set invalid password
        var invalidPassword = "abc";

        // Reset password with invalid password
        await _httpClient.PostResetPasswordAsync(resetLink.ApplicationUserId, resetLink.ResetToken, invalidPassword);

        // Retry with valid password
        var resetPasswordResponse = await _httpClient.PostResetPasswordAsync(resetLink.ApplicationUserId, resetLink.ResetToken);
        Assert.Equal(HttpStatusCode.OK, resetPasswordResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenSuccessfull_ShouldRevokeExistingRefreshTokens()
    {
        // Register customer
        var email = await _httpClient.RegisterCustomerAsync();
        // Get authentication data
        var authResponse = await _httpClient.LoginAsync(email);
        // Request password reset link
        await _httpClient.PostForgotPasswordAsync(email);
        // Get reset link
        var resetLink = _factory.PasswordResetEmailsSentTo(email).Last().GetPasswordResetLink();

        // Request password reset
        var resetPasswordResponse = await _httpClient.PostResetPasswordAsync(resetLink.ApplicationUserId, resetLink.ResetToken);
        Assert.Equal(HttpStatusCode.OK, resetPasswordResponse.StatusCode);

        // Using an old refresh token after a password change should fail
        var refreshTokenResponse = await _httpClient.PostRefreshAsync(authResponse.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshTokenResponse.StatusCode);

        // The refresh token should be revoked
        var refreshToken = (await _factory.GetStoredRefreshTokenAsync(authResponse.RefreshToken))!;
        Assert.True(refreshToken.IsRevoked);
    }

    [Theory]
    [InlineData("", "Password1!", "Password1!")] // Reset token is empty
    [InlineData("reset-token", "abc", "abc")] // New password is too short
    [InlineData("reset-token", "", "Password1!")] // New password is empty
    [InlineData("reset-token", "Password1!", "")] // Confirm password is empty
    [InlineData("reset-token", "Password1!", "AnotherPassword1!")] // Password mismatch
    public async Task ResetPassword_WhenInvalidRequest_ShouldReturn400BadRequest(string resetToken, string newPassword, string confirmPassword)
    {
        // Since the validation of parameters runs before handler execute - we can pass any user id
        var resetPasswordResponse = await _httpClient.PostResetPasswordAsync(Guid.NewGuid(), resetToken, newPassword, confirmPassword);

        Assert.Equal(HttpStatusCode.BadRequest, resetPasswordResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenUserDoesNotExist_ShouldReturn404NotFound()
    {
        var resetPasswordResponse = await _httpClient.PostResetPasswordAsync(Guid.NewGuid(), "reset-token");

        Assert.Equal(HttpStatusCode.NotFound, resetPasswordResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenResetTokenRequestedTwice_ShouldInvalidateOtherTokenAfterReset()
    {
        var email = await _httpClient.RegisterCustomerAsync();

        // Send the password reset link twice
        await _httpClient.PostForgotPasswordAsync(email);
        await _httpClient.PostForgotPasswordAsync(email);

        // Extract the password reset links from the emails
        var passwordResetLinks = _factory.PasswordResetEmailsSentTo(email)
            .Select(e => e.GetPasswordResetLink())
            .ToList();
        Assert.Equal(2, passwordResetLinks.Count);

        var firstResetLink = passwordResetLinks[0];
        var secondResetLink = passwordResetLinks[1];

        // Send the password reset requests
        var firstPasswordResetResponse = await _httpClient.PostResetPasswordAsync(firstResetLink.ApplicationUserId, firstResetLink.ResetToken);
        var secondPasswordResetResponse = await _httpClient.PostResetPasswordAsync(secondResetLink.ApplicationUserId, secondResetLink.ResetToken);

        // Assert that after using the first password reset token we can't use the second one
        Assert.Equal(HttpStatusCode.OK, firstPasswordResetResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, secondPasswordResetResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenResetIsSuccessfull_ShouldNotRevokeRefreshTokensOfOtherUser()
    {
        var otherEmail = await _httpClient.RegisterCustomerAsync();
        var otherAuthResponse = await _httpClient.LoginAsync(otherEmail);

        // Register another customer and request a forgot password email
        var (_, resetLink) = await RequestForgotPasswordAsync();
        var resetPasswordResponse = await _httpClient.PostResetPasswordAsync(resetLink.ApplicationUserId, resetLink.ResetToken);
        Assert.Equal(HttpStatusCode.OK, resetPasswordResponse.StatusCode);

        // Check if the other customer can refresh, it should pass since password reset for the first user
        // should not revoke refresh token for the second customer
        var refreshResponse = await _httpClient.PostRefreshAsync(otherAuthResponse.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
    }
}
