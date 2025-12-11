namespace AuthTemplate.Models.Options;

public sealed class InitialAdminOptions
{
    public const string InitialAdminOptionsKey = "InitialAdminOptions"; // AKA The name of Cors section in settings
    public bool AddInitialAdmin { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
