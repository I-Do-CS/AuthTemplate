using System.Linq.Expressions;

namespace AuthTemplate.Models.Role;

public sealed record RoleDto
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string NormalizedName { get; init; }

    public static RoleDto CreateFromEntity(Entities.Role entity)
    {
        return new RoleDto
        {
            Id = entity.Id,
            Name = entity.Name!,
            NormalizedName = entity.NormalizedName!,
        };
    }

    public static Expression<Func<Entities.Role, RoleDto>> ProjectFromEntity()
    {
        return entity => new RoleDto
        {
            Id = entity.Id,
            Name = entity.Name!,
            NormalizedName = entity.NormalizedName!,
        };
    }
}
