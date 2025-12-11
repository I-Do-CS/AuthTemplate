using AuthTemplate.Models.Role;
using AuthTemplate.Models.User;

namespace AuthTemplate.Services.Abstractions;

public interface IAdministratorService
{
    Task<UserWithRolesDto> GetUserByIdAsync(string id);

    Task<UserWithRolesDto> GetUserByEmailAsync(string email);

    Task<UsersCollectionResponse> GetUsersAsync(UsersQueryParameters parameters);

    Task<UsersCollectionResponse> GetAdminsAsync(UsersQueryParameters parameters);

    Task<RolesCollectionResponse> GetRolesAsync(RolesQueryParameters parameters);

    Task<UserWithRolesDto> PromoteToAdminById(string id);

    Task<UserWithRolesDto> PromoteToAdminByEmail(string email);

    Task<UserWithRolesDto> DemoteFromAdminById(string id);

    Task<UserWithRolesDto> DemoteFromAdminByEmail(string email);

    Task<UserWithRolesDto> RevokeRefreshTokenByIdAsync(string id);

    Task<UserWithRolesDto> RevokeRefreshTokenByEmailAsync(string email);

    Task<UserWithRolesDto> ResetPasswordAsync(
        UserResetPasswordRequest request,
        UserResetPasswordRequestValidator validator
    );

    Task<UserWithRolesDto> RevertSoftDeleteByIdAsync(string id);

    Task<UserWithRolesDto> RevertSoftDeleteByEmailAsync(string email);

    Task DeleteUserByIdAsync(string id);

    Task DeleteUserByEmailAsync(string email);
}
