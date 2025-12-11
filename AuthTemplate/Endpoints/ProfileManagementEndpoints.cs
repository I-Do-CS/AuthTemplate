using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthTemplate.Endpoints.Internal;
using AuthTemplate.Models.User;
using AuthTemplate.Services;
using AuthTemplate.Services.Abstractions;
using AuthTemplate.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthTemplate.Endpoints;

public sealed class ProfileManagementEndpoints : IEndpoints
{
    private const string BaseRoute = "api/profile";
    private const string Tag = "Profile";
    private const string ContentType = "application/json";

    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet($"{BaseRoute}", GetProfile)
            .WithName("GetProfile")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(Tag)
            .WithSummary("Profile")
            .WithDescription(
                """
                    Gets currently logged in user's profile data.
                """
            );

        app.MapPut($"{BaseRoute}", UpdateProfile)
            .WithName("UpdateProfile")
            .Accepts<UserUpdateRequest>(ContentType)
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(Tag)
            .WithSummary("Update")
            .WithDescription(
                """
                    Accepts an update object and updates
                    currently logged in user's data with it.
                    To keep fields unchanged, send duplicate
                    or null values.
                    Remember to refresh tokens after successful
                    update requests.
                """
            );

        app.MapPut($"{BaseRoute}/change-email", ChangeEmail)
            .WithName("ChangeEmail")
            .Accepts<UserChangeEmailRequest>(ContentType)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithTags(Tag)
            .WithSummary("Change Email")
            .WithDescription(
                """
                If successful, changes currently logged in user's
                email and logs them out so they have to login again
                renew their tokens.
                """
            );

        app.MapPut($"{BaseRoute}/change-password", ChangePassword)
            .WithName("ChangePassword")
            .Accepts<UserChangePasswordRequest>(ContentType)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithTags(Tag)
            .WithSummary("Change Password")
            .WithDescription(
                """
                If successful, changes currently logged in user's
                password and logs them out so they have to login again
                renew their tokens.
                """
            );

        app.MapPut($"{BaseRoute}/deactivate", Deactivate)
            .WithName("DeactivateProfile")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags(Tag)
            .WithSummary("Deactivate Profile")
            .WithDescription(
                $"""
                Changes '{nameof(UserDto.IsDeactive)}' flag for
                the currently logged in user's profile to 'true'
                and logs them out.
                This has no functional effects on the api and its
                behaviour; has to be enforced client-side.
                User profiles with the 'Admin' role cannot be
                deactivated.
                """
            );

        app.MapPut($"{BaseRoute}/reactivate", Reactivate)
            .WithName("ReactivateProfile")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(Tag)
            .WithSummary("Reactivate Profile")
            .WithDescription(
                $"""
                Changes '{nameof(UserDto.IsDeactive)}' flag for
                the currently logged in user's profile to 'false'.
                This has no functional effects on the api and its
                behaviour; has to be enforced client-side.
                """
            );

        app.MapPut($"{BaseRoute}/delete", SoftDelete)
            .WithName("SoftDeleteProfile")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags(Tag)
            .WithSummary("Soft Delete Profile")
            .WithDescription(
                $"""
                Changes '{nameof(UserDto.IsDeleted)}' flag for
                the currently logged in user's profile to 'true'
                and logs them out.
                This has no functional effects on the api and its
                behaviour; has to be enforced client-side.
                User profiles with the 'Admin' role cannot be
                soft deleted.
                Reverting this change and actual profile deletion
                are admin only operations defined in the 'Admin' endpoints.
                """
            );
    }

    [Authorize(Roles = AuthConstants.AllRoles)]
    internal static async Task<IResult> GetProfile(
        [FromServices] IProfileManagementService profile,
        ClaimsPrincipal claims
    )
    {
        var id = GetIdFromClaims(claims);
        return Results.Ok(await profile.GetProfileAsync(id));
    }

    [Authorize(Roles = AuthConstants.AllRoles)]
    internal static async Task<IResult> UpdateProfile(
        [FromBody] UserUpdateRequest request,
        [FromServices] UserUpdateRequestValidator validator,
        [FromServices] IProfileManagementService profile,
        ClaimsPrincipal claims
    )
    {
        var id = GetIdFromClaims(claims);
        var result = await profile.UpdateProfileAsync(id, request, validator);
        return Results.Ok(result);
    }

    [Authorize(Roles = AuthConstants.AllRoles)]
    internal static async Task<IResult> ChangeEmail(
        [FromBody] UserChangeEmailRequest request,
        [FromServices] UserChangeEmailRequestValidator validator,
        [FromServices] IProfileManagementService profile,
        [FromServices] IAuthenticationService auth,
        ClaimsPrincipal claims,
        HttpResponse response
    )
    {
        var id = GetIdFromClaims(claims);
        await profile.ChangeEmailAsync(id, request, validator);
        await auth.LogoutAsync(id, response.Cookies);
        return Results.NoContent();
    }

    [Authorize(Roles = AuthConstants.AllRoles)]
    internal static async Task<IResult> ChangePassword(
        [FromBody] UserChangePasswordRequest request,
        [FromServices] UserChangePasswordRequestValidator validator,
        [FromServices] IProfileManagementService profile,
        [FromServices] IAuthenticationService auth,
        ClaimsPrincipal claims,
        HttpResponse response
    )
    {
        var id = GetIdFromClaims(claims);
        await profile.ChangePasswordAsync(id, request, validator);
        await auth.LogoutAsync(id, response.Cookies);
        return Results.NoContent();
    }

    [Authorize(Roles = AuthConstants.AllRoles)]
    internal static async Task<IResult> Deactivate(
        [FromServices] IProfileManagementService profile,
        ClaimsPrincipal claims
    )
    {
        var id = GetIdFromClaims(claims);
        await profile.DeactivateProfileAsync(id);
        return Results.NoContent();
    }

    [Authorize(Roles = AuthConstants.AllRoles)]
    internal static async Task<IResult> Reactivate(
        [FromServices] IProfileManagementService profile,
        ClaimsPrincipal claims
    )
    {
        var id = GetIdFromClaims(claims);
        await profile.ReactivateProfileAsync(id);
        return Results.NoContent();
    }

    [Authorize(Roles = AuthConstants.AllRoles)]
    internal static async Task<IResult> SoftDelete(
        [FromServices] IProfileManagementService profile,
        ClaimsPrincipal claims
    )
    {
        var id = GetIdFromClaims(claims);
        await profile.SoftDeleteProfileAsync(id);
        return Results.NoContent();
    }

    private static string GetIdFromClaims(ClaimsPrincipal claims)
    {
        return claims.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;
    }

    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProfileManagementService, ProfileManagementService>();
    }
}
