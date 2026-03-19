// !!! ATTENTION !!!
// At this point the entire application is not structured
// the architecture pattern comes pretty soon
// at this point I've implemented the JWT authentication logic :D

using System.Security.Claims;
using System.Text;
using BookRest.Api.Constants;
using BookRest.Api.Data;
using BookRest.Api.Features;
using BookRest.Api.Models;
using BookRest.Api.Services;
using BookRest.Api.Startup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Extract to the separate file
builder.Services.AddHttpContextAccessor();
builder.AddDependencies();
builder.AddDatabase();

// Extract to the separate class
// AuthConfig
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:Issuer"];
        options.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:Audience"];
        options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
        );
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.MigrateDb();

await app.InitializeAsync();

app.UseDevelopmentServices();

app.UseHttpsRedirection();

app.MapControllers();

// The following endpoints are for testing purposes only

// Extract to the Controllers
// Just for testing
// Maybe specify the current API version here?
app.MapGet("/", () => "hello bookrest");

app.MapDelete(
        "users/{id}/refresh-tokens",
        async (string id, RevokeRefreshTokens useCase) =>
        {
            bool success = await useCase.Handle(id);
            return success ? Results.NoContent() : Results.BadRequest();
        }
    )
    .RequireAuthorization();

app.MapPost(
    "users/refresh-token",
    async (LoginUserWithRefreshToken.Request request, LoginUserWithRefreshToken useCase) =>
    {
        var result = await useCase.Handle(request);
        return Results.Ok(result);
    }
);

// Just for testing
// Extract to the separate AuthController?
app.MapPost(
    "/register",
    async (
        RegisterUser.Request request,
        UserManager<AppUser> userManager,
        RegisterUser registerUser
    ) =>
    {
        var result = await registerUser.Handle(request);
        return Results.Ok(result);
    }
);

// Just for testing
// Extract to the AuthController
app.MapPost(
    "/login",
    async (LoginUser.Request request, UserManager<AppUser> userManager, LoginUser loginUser) =>
    {
        var result = await loginUser.Handle(request);
        return Results.Ok(result);
    }
);

// Just for testing
// Extract to the separate UserController
// GetUserById endpoint
app.MapGet(
        "me",
        (ClaimsPrincipal claimsPrincipal) =>
        {
            return Results.Ok(claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value));
        }
    )
    .RequireAuthorization(policy => policy.RequireRole(Roles.Member));

app.UseAuthentication();

app.UseAuthorization();

app.Run();

