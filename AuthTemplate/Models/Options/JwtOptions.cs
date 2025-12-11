namespace AuthTemplate.Models.Options;

public sealed class JwtOptions
{
    public const string JwtOptionsKey = "JwtOptions"; // AKA The name of jwt section in settings
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationTimeInMinutes { get; set; }
    public int RefreshTokenExpirationTimeInDays {  get; set; }
}
