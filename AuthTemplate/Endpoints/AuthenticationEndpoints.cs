using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthTemplate.Endpoints.Internal;
using AuthTemplate.Models.User;
using AuthTemplate.Services;
using AuthTemplate.Services.Abstractions;
using AuthTemplate.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthTemplate.Endpoints;

public sealed class AuthenticationEndpoints : IEndpoints
{
    private const string BaseRoute = "api/auth";
    private const string Tag = "Authentication";
    private const string ContentType = "application/json";

    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost($"{BaseRoute}/register", Register)
            .WithName("Register")
            .Accepts<UserRegisterRequest>(ContentType)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithTags(Tag)
            .WithSummary("Register");

        app.MapPost($"{BaseRoute}/login", Login)
            .WithName("Login")
            .Accepts<UserLoginRequest>(ContentType)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithTags(Tag)
            .WithSummary("Login")
            .WithDescription(
                $"""
                Grants an {AuthConstants.AccessToken} and a {AuthConstants.RefreshToken}.
                Stores them in 'Secure', 'HttpOnly', 'SameSite: Strict' cookies.
                """
            );

        app.MapPost($"{BaseRoute}/refresh", Refresh)
            .WithName("Refresh")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithTags(Tag)
            .WithSummary("Refresh")
            .WithDescription(
                $"""
                Grants a new {AuthConstants.AccessToken} and a {AuthConstants.RefreshToken}. 
                Rotates the old refresh token.
                """
            );

        app.MapPost($"{BaseRoute}/logout", Logout)
            .WithName("Logout")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(Tag)
            .WithSummary("Logout")
            .WithDescription(
                $"""
                Removes {AuthConstants.AccessToken} and {AuthConstants.RefreshToken}
                from the cookie jar and revokes the refresh token internally.
                """
            );
    }

    [AllowAnonymous]
    internal static async Task<IResult> Register(
        [FromBody] UserRegisterRequest request,
        [FromServices] UserRegisterRequestValidator validator,
        [FromServices] IAuthenticationService auth
    )
    {
        await auth.RegisterAsync(request, validator);
        return Results.Ok();
    }

    [AllowAnonymous]
    internal static async Task<IResult> Login(
        [FromBody] UserLoginRequest request,
        [FromServices] UserLoginRequestValidator validator,
        [FromServices] IAuthenticationService auth,
        HttpResponse response
    )
    {
        await auth.LoginAsync(response.Cookies, request, validator);
        return Results.Ok();
    }

    [AllowAnonymous]
    internal static async Task<IResult> Refresh(
        [FromServices] IAuthenticationService auth,
        HttpRequest request,
        HttpResponse response
    )
    {
        await auth.RefreshTokenAsync(request.Cookies, response.Cookies);
        return Results.Ok();
    }

    [Authorize(Roles = AuthConstants.AllRoles)]
    internal static async Task<IResult> Logout(
        [FromServices] IAuthenticationService auth,
        ClaimsPrincipal user,
        HttpResponse response
    )
    {
        var id = user.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;
        await auth.LogoutAsync(id, response.Cookies);
        return Results.Ok();
    }

    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
    }
}
