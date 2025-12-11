using AuthTemplate.Models.User;

namespace AuthTemplate.Services.Abstractions;

public interface IProfileManagementService
{
    Task<UserDto> GetProfileAsync(string userId);

    Task<UserDto> UpdateProfileAsync(
        string userId,
        UserUpdateRequest request,
        UserUpdateRequestValidator validator
    );

    Task<UserDto> ChangeEmailAsync(
        string userId,
        UserChangeEmailRequest request,
        UserChangeEmailRequestValidator validator
    );

    Task<UserDto> ChangePasswordAsync(
        string userId,
        UserChangePasswordRequest request,
        UserChangePasswordRequestValidator validator
    );

    Task<UserDto> DeactivateProfileAsync(string id);

    Task<UserDto> ReactivateProfileAsync(string id);

    Task<UserDto> SoftDeleteProfileAsync(string id);
}
