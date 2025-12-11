using AuthTemplate.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace AuthTemplate.Models.Role;

public sealed record RolesCollectionResponse : CollectionResponse<RoleDto>
{
    public static async Task<RolesCollectionResponse> CreateAsync(
        IQueryable<RoleDto> query,
        int page,
        int pageSize
    )
    {
        int totalCount = await query.CountAsync();

        List<RoleDto> items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new RolesCollectionResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
