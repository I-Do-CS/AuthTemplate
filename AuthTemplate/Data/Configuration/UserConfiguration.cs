using AuthTemplate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthTemplate.Data.Configuration;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Id).HasMaxLength(256);
        builder.Property(u => u.FirstName).HasMaxLength(256);
        builder.Property(u => u.LastName).HasMaxLength(256);
    }
}
