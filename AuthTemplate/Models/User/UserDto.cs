using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AuthTemplate.Models.Role;

namespace AuthTemplate.Models.User;

public sealed record UserDto
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

    public static UserDto CreateFromEntity(Entities.User entity)
    {
        return new UserDto
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
        };
    }

    public static Expression<Func<Entities.User, UserDto>> ProjectFromEntity()
    {
        return entity => new UserDto
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
        };
    }
}
