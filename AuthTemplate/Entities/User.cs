using Microsoft.AspNetCore.Identity;

namespace AuthTemplate.Entities;

public sealed class User : IdentityUser<string>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public DateOnly? DateOfBirthUtc { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }

    public bool IsDeleted { get; set; } // Soft Delete
    public bool IsDeactive { get; set; } // Account Deactivation

    public static User Create(string email, string firstName, string lastName)
    {
        return new User
        {
            Id = "u_" + Guid.CreateVersion7().ToString(),
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAtUtc = DateTime.UtcNow,
            IsDeleted = false,
            IsDeactive = false,
            DateOfBirthUtc = null,
        };
    }

    public override string ToString()
    {
        return FirstName + " " + LastName;
    }
}
