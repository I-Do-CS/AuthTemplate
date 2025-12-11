namespace AuthTemplate.Models.Role;

// Everything is nullable and the default values are enforced in
// the service layer because MinimalAPI's POS binding system
// doesn't properly behave with complex types for some reason!
public sealed class RolesQueryParameters
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? Search { get; set; }
    public bool? OrderByName { get; set; }
}
