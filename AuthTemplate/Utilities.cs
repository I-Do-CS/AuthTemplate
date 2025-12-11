using AuthTemplate.Data;
using AuthTemplate.Entities;
using AuthTemplate.Models.Options;
using AuthTemplate.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace AuthTemplate;

public static class Utilities
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        await using ApplicationDbContext applicationDbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            await applicationDbContext.Database.MigrateAsync();
            Log.Information("Application database migration applied successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error ocurred while applying database migrations");
            throw;
        }
    }

    /// <summary>
    /// Ensure AuthRolenamesConstants.AsList exist in db
    /// </summary>
    public static async Task EnsureRolesCreatedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Ensure standard roles exist
        var roles = AuthConstants.RolesList;
        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var createResult = await roleManager.CreateAsync(
                new Role { Id = $"role_{Guid.CreateVersion7()}", Name = role }
            );
            if (!createResult.Succeeded)
            {
                logger.LogError(
                    "Failed to create role {Role}: {Errors}",
                    role,
                    string.Join(", ", createResult.Errors.Select(e => e.Description))
                );
            }
        }
    }

    /// <summary>
    /// Create the initial admin
    /// </summary>
    public static async Task EnsureAdminCreatedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var userManager = services.GetRequiredService<UserManager<User>>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        var initialAdminOptions = services
            .GetRequiredService<IOptions<InitialAdminOptions>>()
            .Value;

        if (!initialAdminOptions.AddInitialAdmin)
        {
            return;
        }

        // Seed an initial admin if configured. Use secure config sources (env vars / Key Vault) in production.
        var adminEmail = initialAdminOptions.Email;
        var adminPassword = initialAdminOptions.Password;
        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = User.Create(adminEmail, "System", "Administrator");
                admin.PasswordHash = userManager.PasswordHasher.HashPassword(admin, adminPassword);

                var create = await userManager.CreateAsync(admin, adminPassword);
                if (create.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, AuthConstants.User);
                    await userManager.AddToRoleAsync(admin, AuthConstants.Admin);
                    logger.LogInformation("Seeded initial admin user: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError(
                        "Failed to create initial admin {Email}: {Errors}",
                        adminEmail,
                        string.Join(", ", create.Errors.Select(e => e.Description))
                    );
                }
                return;
            }
            if (!await userManager.IsInRoleAsync(admin, AuthConstants.Admin))
            {
                await userManager.AddToRoleAsync(admin, AuthConstants.Admin);
            }
            if (!await userManager.IsInRoleAsync(admin, AuthConstants.User))
            {
                await userManager.AddToRoleAsync(admin, AuthConstants.User);
            }
        }
    }
}
