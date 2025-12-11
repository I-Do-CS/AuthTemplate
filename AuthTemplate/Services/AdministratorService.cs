using AuthTemplate.Data;
using AuthTemplate.Entities;
using AuthTemplate.Exceptions;
using AuthTemplate.Models.Role;
using AuthTemplate.Models.User;
using AuthTemplate.Services.Abstractions;
using AuthTemplate.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AuthTemplate.Services;

public sealed class AdministratorService(
    UserManager<User> userManager,
    ApplicationDbContext dbContext
) : IAdministratorService
{
    public async Task<UserWithRolesDto> GetUserByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new BadRequestException($"{nameof(id)} parameter cannot be empty or whitespace.");
        }

        var (user, roles) = await GetUserAndRolesByIdAsync(id);
        if (user is null)
        {
            throw new NotFoundException($"User with the id of {id} was not found");
        }
        return UserWithRolesDto.CreateFromEntity(user, roles);
    }

    public async Task<UserWithRolesDto> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException(
                $"{nameof(email)} parameter cannot be empty or whitespace."
            );
        }

        var (user, roles) = await GetUserAndRolesByEmailAsync(email);
        if (user is null)
        {
            throw new NotFoundException($"User with the email of {email} was not found");
        }
        return UserWithRolesDto.CreateFromEntity(user, roles);
    }

    public async Task<UsersCollectionResponse> GetUsersAsync(UsersQueryParameters parameters)
    {
        parameters = EnforceDefaultValuesForUserParams(parameters);

        if (parameters.Page < 1 || parameters.PageSize < 1)
        {
            throw new BadRequestException(
                $"{nameof(parameters.Page)} and {nameof(parameters.Page)} must be equal or more than 1"
            );
        }

        var query = dbContext.Users.AsQueryable();

        if (parameters.Search is not null)
        {
            query = query.Where(u =>
                u.UserName!.ToLower().Contains(parameters.Search.ToLower())
                || u.FirstName.ToLower().Contains(parameters.Search.ToLower())
                || u.LastName.ToLower().Contains(parameters.Search.ToLower())
                || u.Email!.ToLower().Contains(parameters.Search.ToLower())
            );
        }

        query = (bool)parameters.IsDeleted! ? query.Where(u => u.IsDeleted) : query;
        query = (bool)parameters.IsDeactive! ? query.Where(u => u.IsDeactive) : query;

        query = (bool)parameters.OrderByName!
            ? query.OrderBy(u => u.FirstName)
            : query.OrderBy(u => u.Id);

        var users = query.Select(u => new
        {
            User = u,
            Roles = (
                from ur in dbContext.UserRoles
                join r in dbContext.Roles on ur.RoleId equals r.Id
                where ur.UserId == u.Id
                select r.Name
            ).ToList(),
        });

        var dtosQuery = users.Select(x => UserWithRolesDto.CreateFromEntity(x.User, x.Roles));

        var response = await UsersCollectionResponse.CreateAsync(
            dtosQuery,
            (int)parameters.Page!,
            (int)parameters.PageSize!
        );

        return response;
    }

    public async Task<UsersCollectionResponse> GetAdminsAsync(UsersQueryParameters parameters)
    {
        parameters = EnforceDefaultValuesForUserParams(parameters);

        if (parameters.Page < 1 || parameters.PageSize < 1)
        {
            throw new BadRequestException(
                $"{nameof(parameters.Page)} and {nameof(parameters.Page)} must be equal or more than 1"
            );
        }

        var query = dbContext.Users.AsQueryable();

        if (parameters.Search is not null)
        {
            query = query.Where(u =>
                u.UserName!.ToLower().Contains(parameters.Search.ToLower())
                || u.FirstName.ToLower().Contains(parameters.Search.ToLower())
                || u.LastName.ToLower().Contains(parameters.Search.ToLower())
                || u.Email!.ToLower().Contains(parameters.Search.ToLower())
            );
        }

        query = (bool)parameters.IsDeleted! ? query.Where(u => u.IsDeleted) : query;
        query = (bool)parameters.IsDeactive! ? query.Where(u => u.IsDeactive) : query;

        query = (bool)parameters.OrderByName!
            ? query.OrderBy(u => u.FirstName)
            : query.OrderBy(u => u.Id);

        var users = query.Select(u => new
        {
            User = u,
            Roles = (
                from ur in dbContext.UserRoles
                join r in dbContext.Roles on ur.RoleId equals r.Id
                where ur.UserId == u.Id
                select r.Name
            ).ToList(),
        });

        var dtosQuery = users
            .Where(x => x.Roles.Contains(AuthConstants.Admin))
            .Select(x => UserWithRolesDto.CreateFromEntity(x.User, x.Roles));

        var response = await UsersCollectionResponse.CreateAsync(
            dtosQuery,
            (int)parameters.Page!,
            (int)parameters.PageSize!
        );

        return response;
    }

    public async Task<RolesCollectionResponse> GetRolesAsync(RolesQueryParameters parameters)
    {
        parameters = EnforceDefaultValuesForRoleParams(parameters);

        var query = dbContext.Roles.Select(RoleDto.ProjectFromEntity());

        if (parameters.Search is not null)
        {
            query = query.Where(r =>
                r.Name.ToLower().Contains(parameters.Search.ToLower())
            );
        }
        query = (bool)parameters.OrderByName!
            ? query.OrderBy(r => r.Name)
            : query.OrderBy(r => r.Id);

        var response = await RolesCollectionResponse.CreateAsync(
            query,
            (int)parameters.Page!,
            (int)parameters.PageSize!
        );

        return response;
    }

    public async Task<UserWithRolesDto> PromoteToAdminById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new BadRequestException($"{nameof(id)} parameter cannot be empty or whitespace.");
        }

        var (user, roles) = await GetUserAndRolesByIdAsync(id);
        if (user is null)
        {
            throw new NotFoundException($"User with the id of {id} was not found");
        }

        if (!roles.Contains(AuthConstants.Admin))
        {
            await userManager.AddToRoleAsync(user, AuthConstants.Admin);
            roles.Add(AuthConstants.Admin);
        }

        return UserWithRolesDto.CreateFromEntity(user, roles);
    }

    public async Task<UserWithRolesDto> PromoteToAdminByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException(
                $"{nameof(email)} parameter cannot be empty or whitespace."
            );
        }

        var (user, roles) = await GetUserAndRolesByEmailAsync(email);
        if (user is null)
        {
            throw new NotFoundException($"User with the email of {email} was not found");
        }

        if (!roles.Contains(AuthConstants.Admin))
        {
            await userManager.AddToRoleAsync(user, AuthConstants.Admin);
            roles.Add(AuthConstants.Admin);
        }

        return UserWithRolesDto.CreateFromEntity(user, roles);
    }

    public async Task<UserWithRolesDto> DemoteFromAdminById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new BadRequestException($"{nameof(id)} parameter cannot be empty or whitespace.");
        }

        var (user, roles) = await GetUserAndRolesByIdAsync(id);
        if (user is null)
        {
            throw new NotFoundException($"User with the id of {id} was not found");
        }

        if (roles.Contains(AuthConstants.Admin))
        {
            await userManager.RemoveFromRoleAsync(user, AuthConstants.Admin);
            roles.Remove(AuthConstants.Admin);
        }

        return UserWithRolesDto.CreateFromEntity(user, roles);
    }

    public async Task<UserWithRolesDto> DemoteFromAdminByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException(
                $"{nameof(email)} parameter cannot be empty or whitespace."
            );
        }

        var (user, roles) = await GetUserAndRolesByEmailAsync(email);
        if (user is null)
        {
            throw new NotFoundException($"User with the email of {email} was not found");
        }

        if (roles.Contains(AuthConstants.Admin))
        {
            await userManager.RemoveFromRoleAsync(user, AuthConstants.Admin);
            roles.Remove(AuthConstants.Admin);
        }

        return UserWithRolesDto.CreateFromEntity(user, roles);
    }

    public async Task<UserWithRolesDto> RevokeRefreshTokenByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new BadRequestException($"{nameof(id)} parameter cannot be empty or whitespace.");
        }

        var (user, roles) = await GetUserAndRolesByIdAsync(id);
        if (user is null)
        {
            throw new NotFoundException($"User with the id of {id} was not found");
        }

        user.RefreshToken = null;
        await userManager.UpdateAsync(user);
        return UserWithRolesDto.CreateFromEntity(user, roles);
    }

    public async Task<UserWithRolesDto> RevokeRefreshTokenByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException(
                $"{nameof(email)} parameter cannot be empty or whitespace."
            );
        }

        var (user, roles) = await GetUserAndRolesByEmailAsync(email);
        if (user is null)
        {
            throw new NotFoundException($"User with the email of {email} was not found");
        }

        user.RefreshToken = null;
        await userManager.UpdateAsync(user);
        return UserWithRolesDto.CreateFromEntity(user, roles);
    }

    public async Task<UserWithRolesDto> ResetPasswordAsync(
        UserResetPasswordRequest request,
        UserResetPasswordRequestValidator validator
    )
    {
        await validator.ValidateAndThrowAsync(request);

        var (user, roles) = await GetUserAndRolesByEmailAsync(request.Email);
        if (user is null)
        {
            throw new NotFoundException($"User with the email of {request.Email} was not found");
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
        if (!result.Succeeded)
        {
            Log.Error("Password reset operation failed. Errors: {@Errors}", result.Errors);
            throw new BadRequestException("Password reset operation failed. Try again later.");
        }

        user.RefreshToken = null;
        await userManager.UpdateAsync(user);
        return UserWithRolesDto.CreateFromEntity(user, roles);
    }

    public async Task<UserWithRolesDto> RevertSoftDeleteByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new BadRequestException($"{nameof(id)} parameter cannot be empty or whitespace.");
        }

        var (user, roles) = await GetUserAndRolesByIdAsync(id);
        if (user is null)
        {
            throw new NotFoundException($"User with the id of {id} was not found");
        }

        user.IsDeleted = false;
        await userManager.UpdateAsync(user);
        return UserWithRolesDto.CreateFromEntity(user, roles);
    }

    public async Task<UserWithRolesDto> RevertSoftDeleteByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException(
                $"{nameof(email)} parameter cannot be empty or whitespace."
            );
        }

        var (user, roles) = await GetUserAndRolesByEmailAsync(email);
        if (user is null)
        {
            throw new NotFoundException($"User with the email of {email} was not found");
        }

        user.IsDeleted = false;
        await userManager.UpdateAsync(user);
        return UserWithRolesDto.CreateFromEntity(user, roles);
    }

    public async Task DeleteUserByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new BadRequestException($"{nameof(id)} parameter cannot be empty or whitespace.");
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            throw new NotFoundException($"User with the id of {id} was not found");
        }

        await userManager.DeleteAsync(user);
    }

    public async Task DeleteUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException(
                $"{nameof(email)} parameter cannot be empty or whitespace."
            );
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            throw new NotFoundException($"User with the email of {email} was not found");
        }

        await userManager.DeleteAsync(user);
    }

    private async Task<(User?, IList<string>)> GetUserAndRolesByIdAsync(string id)
    {
        User? user = await userManager.FindByIdAsync(id);
        IList<string> roles = user is null ? [] : await userManager.GetRolesAsync(user);
        return (user, roles);
    }

    private async Task<(User?, IList<string>)> GetUserAndRolesByEmailAsync(string email)
    {
        User? user = await userManager.FindByEmailAsync(email);
        IList<string> roles = user is null ? [] : await userManager.GetRolesAsync(user);
        return (user, roles);
    }

    private UsersQueryParameters EnforceDefaultValuesForUserParams(UsersQueryParameters parameters)
    {
        parameters.Search = string.IsNullOrWhiteSpace(parameters.Search) ? null : parameters.Search;
        parameters.IsDeactive ??= false;
        parameters.IsDeleted ??= false;
        parameters.OrderByName ??= false;
        parameters.Page ??= 1;
        parameters.PageSize ??= 10;

        return parameters;
    }

    private RolesQueryParameters EnforceDefaultValuesForRoleParams(RolesQueryParameters parameters)
    {
        parameters.Search = string.IsNullOrWhiteSpace(parameters.Search) ? null : parameters.Search;
        parameters.OrderByName ??= false;
        parameters.Page ??= 1;
        parameters.PageSize ??= 10;

        return parameters;
    }
}
