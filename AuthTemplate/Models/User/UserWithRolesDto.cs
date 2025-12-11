using System.Linq.Expressions;
using AuthTemplate.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthTemplate.Models.User;

public sealed record UserWithRolesDto
{
    public string Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public string UserName { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateOnly? DateOfBirthUtc { get; init; }
    public bool IsDeleted { get; init; }
    public bool IsDeactive { get; init; }

    public string[] Roles { get; set; }

    public static UserWithRolesDto CreateFromEntity(Entities.User entity, IEnumerable<string> roles)
    {
        return new UserWithRolesDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            UserName = entity.UserName!,
            Email = entity.Email!,
            CreatedAtUtc = entity.CreatedAtUtc,
            DateOfBirthUtc = entity.DateOfBirthUtc,
            IsDeleted = entity.IsDeleted,
            IsDeactive = entity.IsDeactive,
            Roles = [.. roles],
        };
    }
}
