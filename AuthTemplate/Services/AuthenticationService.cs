using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthTemplate.Data;
using AuthTemplate.Entities;
using AuthTemplate.Exceptions;
using AuthTemplate.Models.Options;
using AuthTemplate.Models.User;
using AuthTemplate.Services.Abstractions;
using AuthTemplate.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthTemplate.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly JwtOptions jwtOptions;
    private readonly AuthTokenProcessor tokenProcessor;
    private readonly UserManager<User> userManager;
    private readonly ApplicationDbContext dbContext;

    public AuthenticationService(
        IOptions<JwtOptions> jwtOptions,
        UserManager<User> userManager,
        ApplicationDbContext dbContext
    )
    {
        this.jwtOptions = jwtOptions.Value;
        this.userManager = userManager;
        this.dbContext = dbContext;
        tokenProcessor = new(jwtOptions.Value, userManager);
    }

    public async Task RegisterAsync(
        UserRegisterRequest request,
        UserRegisterRequestValidator validator
    )
    {
        await validator.ValidateAndThrowAsync(request);

        var userExists = await userManager.FindByEmailAsync(request.Email) is not null;

        if (userExists)
        {
            throw new ConflictException("A user with this email already exists");
        }

        var user = User.Create(request.Email, request.FirstName, request.LastName);
        user.PasswordHash = userManager.PasswordHasher.HashPassword(user, request.Password);

        var result = await userManager.CreateAsync(user);
        await userManager.AddToRoleAsync(user, AuthConstants.User);

        if (!result.Succeeded)
        {
            throw new UnprocessableEntityException("Invalid email or password");
        }
    }

    public async Task LoginAsync(
        IResponseCookies cookies,
        UserLoginRequest request,
        UserLoginRequestValidator validator
    )
    {
        await validator.ValidateAndThrowAsync(request);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedException("Incorrect email or password");
        }

        var (jwtToken, accessTokenExpirationDateInUtc) = await tokenProcessor.GenerateJwtToken(
            user
        );
        var refreshTokenValue = tokenProcessor.GenerateRefreshToken();
        var refreshTokenExpirationDateInUtc = DateTime.UtcNow.AddDays(
            jwtOptions.RefreshTokenExpirationTimeInDays
        );

        user.RefreshToken = refreshTokenValue;
        user.RefreshTokenExpiresAtUtc = refreshTokenExpirationDateInUtc;
        await userManager.UpdateAsync(user);

        tokenProcessor.WriteAuthTokenAsHttpOnlyCookie(
            cookies,
            AuthConstants.AccessToken,
            jwtToken,
            accessTokenExpirationDateInUtc
        );
        tokenProcessor.WriteAuthTokenAsHttpOnlyCookie(
            cookies,
            AuthConstants.RefreshToken,
            user.RefreshToken,
            refreshTokenExpirationDateInUtc
        );
    }

    public async Task RefreshTokenAsync(
        IRequestCookieCollection requestCookies,
        IResponseCookies responseCookies
    )
    {
        var refreshToken = requestCookies[AuthConstants.RefreshToken];
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new UnauthorizedException("Refresh token is revoked or expired");
        }

        var user =
            await dbContext.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken)
            ?? throw new UnauthorizedException("Refresh token is revoked or expired");

        if (user.RefreshTokenExpiresAtUtc < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token is revoked or expired");
        }

        var (jwtToken, accessTokenExpirationDateInUtc) = await tokenProcessor.GenerateJwtToken(
            user
        );
        var refreshTokenValue = tokenProcessor.GenerateRefreshToken();

        var refreshTokenExpirationDateInUtc = DateTime.UtcNow.AddDays(
            jwtOptions.RefreshTokenExpirationTimeInDays
        );

        user.RefreshToken = refreshTokenValue;
        user.RefreshTokenExpiresAtUtc = refreshTokenExpirationDateInUtc;

        await userManager.UpdateAsync(user);

        tokenProcessor.WriteAuthTokenAsHttpOnlyCookie(
            responseCookies,
            AuthConstants.AccessToken,
            jwtToken,
            accessTokenExpirationDateInUtc
        );
        tokenProcessor.WriteAuthTokenAsHttpOnlyCookie(
            responseCookies,
            AuthConstants.RefreshToken,
            user.RefreshToken,
            refreshTokenExpirationDateInUtc
        );
    }

    public async Task RevokeRefreshTokenAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            await tokenProcessor.RevokeRefreshTokenAsync(user);
        }
    }

    public async Task RevokeRefreshTokenAsync(User user)
    {
        await tokenProcessor.RevokeRefreshTokenAsync(user);
    }

    public async Task LogoutAsync(string userId, IResponseCookies cookies)
    {
        var user = await userManager.FindByIdAsync(userId);

        tokenProcessor.RemoveTokenFromContext(cookies, AuthConstants.AccessToken);
        tokenProcessor.RemoveTokenFromContext(cookies, AuthConstants.RefreshToken);
        await RevokeRefreshTokenAsync(user!);
    }

    private sealed class AuthTokenProcessor(JwtOptions jwtOptions, UserManager<User> userManager)
    {
        public async Task<(
            string jwtToken,
            DateTime accessTokenExpirationDateInUtc
        )> GenerateJwtToken(User user)
        {
            var signingkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
            var credentials = new SigningCredentials(signingkey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims =
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Aud, jwtOptions.Audience),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.PreferredUsername, user.UserName ?? user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.ToString()),
                new Claim(ClaimTypes.Role, AuthConstants.User), // Add the user role for all users
            ];

            // Add the admin role for admins
            if (await userManager.IsInRoleAsync(user, AuthConstants.Admin))
            {
                claims.Add(new Claim(ClaimTypes.Role, AuthConstants.Admin));
            }

            var expires = DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenExpirationTimeInMinutes);

            var token = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return (jwtToken, expires);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public void WriteAuthTokenAsHttpOnlyCookie(
            IResponseCookies cookies,
            string cookieName,
            string token,
            DateTime expiration
        )
        {
            cookies.Append(
                cookieName,
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = expiration,
                    IsEssential = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                }
            );
        }

        public void RemoveTokenFromContext(IResponseCookies cookies, string tokenName)
        {
            cookies.Delete(tokenName);
        }

        public async Task RevokeRefreshTokenAsync(User user)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiresAtUtc = null;

            await userManager.UpdateAsync(user);
        }
    }
}
