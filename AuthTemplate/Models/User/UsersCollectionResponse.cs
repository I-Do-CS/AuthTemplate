using AuthTemplate.Models.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthTemplate.Models.User;

public sealed record UsersCollectionResponse : CollectionResponse<UserWithRolesDto>
{
    public static async Task<UsersCollectionResponse> CreateAsync(
        IQueryable<UserWithRolesDto> query,
        int page,
        int pageSize
    )
    {
        int totalCount = await query.CountAsync();

        List<UserWithRolesDto> items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new UsersCollectionResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
