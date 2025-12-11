namespace AuthTemplate.Shared;

public static class AuthConstants
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string AllRoles = $"{Admin}, {User}";
    public static List<string> RolesList => [Admin, User];

    public const string AccessToken = "ACCESS_TOKEN";
    public const string RefreshToken = "REFRESH_TOKEN";
}
