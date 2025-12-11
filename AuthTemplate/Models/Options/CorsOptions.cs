namespace AuthTemplate.Models.Options;

public sealed class CorsOptions
{
    public const string CorsOptionsKey = "CorsOptions"; // AKA The name of Cors section in settings
    public bool AllowAll { get; set; }
    public List<string> AllowedMethods { get; set; } = [];
    public List<string> AllowedOrigins { get; set; } = [];
}
