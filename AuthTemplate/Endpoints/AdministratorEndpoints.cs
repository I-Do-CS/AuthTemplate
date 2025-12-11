using AuthTemplate.Endpoints.Internal;
using AuthTemplate.Models.Role;
using AuthTemplate.Models.User;
using AuthTemplate.Services;
using AuthTemplate.Services.Abstractions;
using AuthTemplate.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthTemplate.Endpoints;

public sealed class AdministratorEndpoints : IEndpoints
{
    private const string BaseRoute = "api";
    private const string Tag = "Administrator";
    private const string ContentType = "application/json";

    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet($"{BaseRoute}/user/{{id}}", GetUserById)
            .WithName("GetUserById")
            .Produces<UserWithRolesDto>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(Tag)
            .WithSummary("Get User by ID");

        app.MapGet($"{BaseRoute}/user", GetUserByEmail)
            .WithName("GetUserByEmail")
            .Produces<UserWithRolesDto>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(Tag)
            .WithSummary("Get User by Email");

        app.MapGet($"{BaseRoute}/users", GetUsers)
            .WithName("GetUsers")
            .Produces<UsersCollectionResponse>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithTags(Tag)
            .WithSummary("Get Users")
            .WithDescription(
                """
                    Gets a paginated list of users. Supports 
                    filtering, searching and minimal sorting as well.
                """
            );

        app.MapGet($"{BaseRoute}/admins", GetAdmins)
            .WithName("GetAdmins")
            .Produces<UsersCollectionResponse>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithTags(Tag)
            .WithSummary("Get Admins")
            .WithDescription(
                """
                    Gets a paginated list of admins. Supports 
                    filtering, searching and minimal sorting as well.
                """
            );

        app.MapGet($"{BaseRoute}/roles", GetRoles)
            .WithName("GetRoles")
            .Produces<UsersCollectionResponse>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithTags(Tag)
            .WithSummary("Get Application Roles")
            .WithDescription(
                """
                    Gets a paginated list of currently available application roles.
                    Supports filtering, searching and minimal sorting as well.
                """
            );

        app.MapPut($"{BaseRoute}/user/promote/{{id}}", PromoteToAdminById)
            .WithName("PromoteById")
            .Produces<UserWithRolesDto>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(Tag)
            .WithSummary("Promote User To Admin by ID")
            .WithDescription(
                """
                    Adds the admin role to a user by id; 
                    effectively creating a new admin.
                """
            );

        app.MapPut($"{BaseRoute}/user/promote", PromoteToAdminByEmail)
            .WithName("PromoteByEmail")
            .Produces<UserWithRolesDto>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(Tag)
            .WithSummary("Promote User To Admin by Email")
            .WithDescription(
                """
                    Adds the admin role to a user by email; 
                    effectively creating a new admin.
                """
            );

        app.MapPut($"{BaseRoute}/user/demote/{{id}}", DemoteFromAdminById)
            .WithName("DemoteById")
            .Produces<UserWithRolesDto>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(Tag)
            .WithSummary("Demote User From Admin by ID")
            .WithDescription(
                """
                    Removes the admin role from a user by id; 
                    Making them a normal user again.
                """
            );

        app.MapPut($"{BaseRoute}/user/demote", DemoteFromAdminByEmail)
            .WithName("DemoteByEmail")
            .Produces<UserWithRolesDto>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(Tag)
            .WithSummary("Demote User From Admin by Email")
            .WithDescription(
                """
                    Removes the admin role from a user by email; 
                    Making them a normal user again.
                """
            );

        app.MapPost($"{BaseRoute}/user/revoke/{{id}}", RevokeRefreshTokenById)
            .WithName("RevokeRefreshTokenById")
            .Produces<UserWithRolesDto>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(Tag)
            .WithSummary("Revoke Refresh Token by ID")
            .WithDescription(
                """
                    Revokes a user's refresh token internally so they have to login again.
                """
            );

        app.MapPost($"{BaseRoute}/user/revoke", RevokeRefreshTokenByEmail)
            .WithName("RevokeRefreshTokenByEmail")
            .Produces<UserWithRolesDto>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(Tag)
            .WithSummary("Revoke Refresh Token by Email")
            .WithDescription(
                """
                    Revokes a user's refresh token internally so they have to login again.
                """
            );

        app.MapPut($"{BaseRoute}/user/reset-password", ResetUserPassword)
            .WithName("ResetUserPassword")
            .Accepts<UserResetPasswordRequest>(ContentType)
            .Produces<UserWithRolesDto>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(Tag)
            .WithSummary("Reset Password by Email")
            .WithDescription(
                """
                Reset a user's password with their email.
                """
            );

        app.MapPut($"{BaseRoute}/user/undelete/{{id}}", RevertSoftDeleteById)
            .WithName("RevertSoftDeleteById")
            .Produces<UserWithRolesDto>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(Tag)
            .WithSummary("Revert Soft Delete by ID")
            .WithDescription(
                """
                Changes '{nameof(UserDto.IsDeleted)}' flag for
                the currently logged in user's profile to 'false'.
                """
            );

        app.MapPut($"{BaseRoute}/user/undelete", RevertSoftDeleteByEmail)
            .WithName("RevertSoftDeleteByEmail")
            .Produces<UserWithRolesDto>(StatusCodes.Status200OK, ContentType)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(Tag)
            .WithSummary("Revert Soft Delete by Email")
            .WithDescription(
                """
                Changes '{nameof(UserDto.IsDeleted)}' flag for
                the currently logged in user's profile to 'false'.
                """
            );

        app.MapDelete($"{BaseRoute}/user/{{id}}", DeleteUserById)
            .WithName("DeleteUserById")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(Tag)
            .WithSummary("Delete User by Id")
            .WithDescription(
                """
                Entirely deletes a User by Id.
                """
            );

        app.MapDelete($"{BaseRoute}/user", DeleteUserByEmail)
            .WithName("DeleteUserByEmail")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(Tag)
            .WithSummary("Delete User by Email")
            .WithDescription(
                """
                Entirely deletes a User by Email.
                """
            );
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> GetUserById(
        [FromRoute] string id,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.GetUserByIdAsync(id));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> GetUserByEmail(
        [FromQuery] string email,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.GetUserByEmailAsync(email));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> GetUsers(
        [AsParameters] UsersQueryParameters parameters,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.GetUsersAsync(parameters));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> GetAdmins(
        [AsParameters] UsersQueryParameters parameters,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.GetAdminsAsync(parameters));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> GetRoles(
        [AsParameters] RolesQueryParameters parameters,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.GetRolesAsync(parameters));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> PromoteToAdminById(
        [FromRoute] string id,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.PromoteToAdminById(id));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> PromoteToAdminByEmail(
        [FromQuery] string email,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.PromoteToAdminByEmail(email));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> DemoteFromAdminById(
        [FromRoute] string id,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.DemoteFromAdminById(id));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> DemoteFromAdminByEmail(
        [FromQuery] string email,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.DemoteFromAdminByEmail(email));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> RevokeRefreshTokenById(
        [FromRoute] string id,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.RevokeRefreshTokenByIdAsync(id));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> RevokeRefreshTokenByEmail(
        [FromQuery] string email,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.RevokeRefreshTokenByEmailAsync(email));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> ResetUserPassword(
        [FromBody] UserResetPasswordRequest request,
        [FromServices] UserResetPasswordRequestValidator validator,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.ResetPasswordAsync(request, validator));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> RevertSoftDeleteById(
        [FromRoute] string id,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.RevertSoftDeleteByIdAsync(id));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> RevertSoftDeleteByEmail(
        [FromQuery] string email,
        [FromServices] IAdministratorService admin
    )
    {
        return Results.Ok(await admin.RevertSoftDeleteByEmailAsync(email));
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> DeleteUserById(
        [FromRoute] string id,
        [FromServices] IAdministratorService admin
    )
    {
        await admin.DeleteUserByIdAsync(id);
        return Results.NoContent();
    }

    [Authorize(Roles = AuthConstants.Admin)]
    internal static async Task<IResult> DeleteUserByEmail(
        [FromQuery] string email,
        [FromServices] IAdministratorService admin
    )
    {
        await admin.DeleteUserByEmailAsync(email);
        return Results.NoContent();
    }

    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAdministratorService, AdministratorService>();
    }
}
