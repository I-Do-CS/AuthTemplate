using AuthTemplate.Entities;
using AuthTemplate.Models.User;

namespace AuthTemplate.Services.Abstractions;

public interface IAuthenticationService
{
    Task LoginAsync(
        IResponseCookies cookies,
        UserLoginRequest request,
        UserLoginRequestValidator validator
    );

    Task LogoutAsync(string userId, IResponseCookies cookies);

    Task RefreshTokenAsync(
        IRequestCookieCollection requestCookies,
        IResponseCookies responseCookies
    );

    Task RegisterAsync(UserRegisterRequest request, UserRegisterRequestValidator validator);

    Task RevokeRefreshTokenAsync(string userId);

    Task RevokeRefreshTokenAsync(User user);
}
