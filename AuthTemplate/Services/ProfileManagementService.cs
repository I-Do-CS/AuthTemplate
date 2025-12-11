using AuthTemplate.Entities;
using AuthTemplate.Exceptions;
using AuthTemplate.Models.User;
using AuthTemplate.Services.Abstractions;
using AuthTemplate.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace AuthTemplate.Services;

public sealed class ProfileManagementService(UserManager<User> userManager)
    : IProfileManagementService
{
    public async Task<UserDto> GetProfileAsync(string userId)
    {
        var (user, _) = await GetUserAndRolesAsync(userId);
        return UserDto.CreateFromEntity(user);
    }

    public async Task<UserDto> UpdateProfileAsync(
        string userId,
        UserUpdateRequest request,
        UserUpdateRequestValidator validator
    )
    {
        await validator.ValidateAndThrowAsync(request);
        var (user, _) = await GetUserAndRolesAsync(userId);

        // Update profile, keep null request values unchanges
        user.FirstName = request.FirstName ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.DateOfBirthUtc = request.DateOfBirthUtc ?? user.DateOfBirthUtc;

        await userManager.UpdateAsync(user);

        return UserDto.CreateFromEntity(user);
    }

    public async Task<UserDto> ChangeEmailAsync(
        string userId,
        UserChangeEmailRequest request,
        UserChangeEmailRequestValidator validator
    )
    {
        await validator.ValidateAndThrowAsync(request);

        var (user, _) = await GetUserAndRolesAsync(userId);
        if (request.NewEmail.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException("New email must be different from the old one");
        }

        var emailChangeToken = await userManager.GenerateChangeEmailTokenAsync(
            user,
            request.NewEmail
        );

        var result = await userManager.ChangeEmailAsync(user, request.NewEmail, emailChangeToken);
        if (!result.Succeeded)
        {
            Log.Error("Email change operation failed. Errors: {@Errors}", result.Errors);
            throw new BadRequestException("Email invalid or already taken");
        }

        // Make sure to change the username as well
        user.UserName = user.Email;
        await userManager.UpdateAsync(user);

        return UserDto.CreateFromEntity(user);
    }

    public async Task<UserDto> ChangePasswordAsync(
        string userId,
        UserChangePasswordRequest request,
        UserChangePasswordRequestValidator validator
    )
    {
        await validator.ValidateAndThrowAsync(request);

        var (user, _) = await GetUserAndRolesAsync(userId);

        // Verify provided old password
        var passwordVerificationResult = userManager.PasswordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash!,
            request.CurrentPassword
        );
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            throw new BadRequestException("The provided current password is incorrect");
        }

        var newPasswordHash = userManager.PasswordHasher.HashPassword(user, request.NewPassword);
        if (newPasswordHash.Equals(user.PasswordHash))
        {
            throw new ConflictException("New password must be different from the old one");
        }

        var result = await userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword
        );
        if (!result.Succeeded)
        {
            Log.Error("Password change operation failed. Errors: {@Errors}", result.Errors);
            throw new ConflictException("Password change failed");
        }

        return UserDto.CreateFromEntity(user);
    }

    public async Task<UserDto> DeactivateProfileAsync(string id)
    {
        var (user, roles) = await GetUserAndRolesAsync(id);
        if (roles.Contains(AuthConstants.Admin))
        {
            throw new ForbiddenException("Admin accounts cannot be deactivated");
        }

        if (!user.IsDeactive)
        {
            user.IsDeactive = true;
            await userManager.UpdateAsync(user);
        }

        return UserDto.CreateFromEntity(user);
    }

    public async Task<UserDto> ReactivateProfileAsync(string id)
    {
        var (user, _) = await GetUserAndRolesAsync(id);

        if (user.IsDeactive)
        {
            user.IsDeactive = false;
            await userManager.UpdateAsync(user);
        }

        return UserDto.CreateFromEntity(user);
    }

    public async Task<UserDto> SoftDeleteProfileAsync(string id)
    {
        var (user, roles) = await GetUserAndRolesAsync(id);
        if (roles.Contains(AuthConstants.Admin))
        {
            throw new ForbiddenException("Admin accounts cannot be soft deleted");
        }

        if (!user.IsDeleted)
        {
            user.IsDeleted = true;
            await userManager.UpdateAsync(user);
        }

        return UserDto.CreateFromEntity(user);
    }

    private async Task<(User, IList<string>)> GetUserAndRolesAsync(string id)
    {
        User user = await userManager.FindByIdAsync(id);
        IList<string> roles = await userManager.GetRolesAsync(user!);
        return (user!, roles);
    }
}
