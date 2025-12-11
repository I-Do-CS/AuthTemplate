using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthTemplate.Data;
using AuthTemplate.Endpoints.Internal;
using AuthTemplate.Entities;
using AuthTemplate.Handlers;
using AuthTemplate.Models.Options;
using AuthTemplate.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace AuthTemplate;

public static class DependencyInjection
{
    /// <summary>
    /// Binds some of the sections in appsettings.json to compatible classes found in ".\Models\Options\"
    /// for easier configuration.
    /// </summary>
    public static WebApplicationBuilder BindApplicationOptions(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<JwtOptions>(
            builder.Configuration.GetSection(JwtOptions.JwtOptionsKey)
        );
        builder.Services.Configure<CorsOptions>(
            builder.Configuration.GetSection(CorsOptions.CorsOptionsKey)
        );
        builder.Services.Configure<InitialAdminOptions>(
            builder.Configuration.GetSection(InitialAdminOptions.InitialAdminOptionsKey)
        );

        return builder;
    }

    /// <summary>
    /// Adds Serilog to WebApplicationBuilder and applies the configurations from appsettings.json.
    /// </summary>
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog(
            (context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            }
        );

        return builder;
    }

    /// <summary>
    /// Adds Open API to services for documentation purposes.
    /// </summary>
    public static WebApplicationBuilder ConfigureApiDocs(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        return builder;
    }

    /// <summary>
    /// Configures CORS policy based on the "CorsOptions" section in appsettings.json.
    /// </summary>
    public static WebApplicationBuilder ConfigureCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            var corsOptions =
                builder.Configuration.GetSection(CorsOptions.CorsOptionsKey).Get<CorsOptions>()
                ?? throw new ArgumentException(nameof(CorsOptions));

            var policyBuilder =
                new Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder().AllowAnyHeader();

            if (corsOptions.AllowAll)
            {
                options.AddDefaultPolicy(
                    policy: policyBuilder.AllowAnyMethod().AllowAnyOrigin().Build()
                );

                return;
            }

            policyBuilder =
                corsOptions.AllowedOrigins.Count == 0
                    ? policyBuilder.AllowAnyOrigin()
                    : policyBuilder.WithOrigins([.. corsOptions.AllowedOrigins]);

            policyBuilder =
                corsOptions.AllowedMethods.Count == 0
                    ? policyBuilder.AllowAnyMethod()
                    : policyBuilder.WithMethods([.. corsOptions.AllowedMethods]);

            options.AddDefaultPolicy(policy: policyBuilder.Build());
        });

        return builder;
    }

    /// <summary>
    /// Adds problem details and global exception handling to services.
    /// </summary>
    public static WebApplicationBuilder ConfigureErrorHandling(this WebApplicationBuilder builder)
    {
        // Add Problem Details
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd(
                    "requestId",
                    context.HttpContext.TraceIdentifier
                );
            };
        });

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        return builder;
    }

    /// <summary>
    /// Adds FluentValidation and any implementations of AbstractValidator found in `Program`'s assembly.
    /// </summary>
    public static WebApplicationBuilder ConfigureValidation(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
        return builder;
    }

    /// <summary>
    /// Adds EfCore for PostgreSQL.
    /// </summary>
    /// <remarks>
    /// I keep my connection string in User Secrets. But any connection string
    /// formatted like "Configuration["ConnectionStrings:PostgreSQL"]" should work.
    /// </remarks>
    public static WebApplicationBuilder ConfigureDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
        });
        return builder;
    }

    /// <summary>
    /// Adds Asp.Net Identity with role based Jwt Authentication and authorization.
    /// On a successful authentication grants the client an "ACCESS_TOKEN" and a "REFRESH_TOKEN"
    /// stored in HttpOnly, Secure, SameSite: Strict cookies. On following requests, repopulates the request
    /// context's token with the aforementioned cookies.
    /// Relevant settings, such as expiry dates can be found in appsettings.json.
    /// Currently, There are 2 roles for authorization; Admin and User found in <see cref="AuthConstants"/>
    /// </summary>
    /// <remarks>
    /// Since rolling your own auth is not best practice and I'm not a security guy,
    /// this auth flow is far from ideal. But for small teams and solo devs such as myself, 
    /// it gets the job done and is secure enough.
    /// </remarks>
    public static WebApplicationBuilder ConfigureAuth(this WebApplicationBuilder builder)
    {
        // Add Asp.Net Identity
        builder
            .Services.AddIdentity<User, Role>(options =>
            {
                // Should also be validated for user/account requests
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Add JWT Authentication
        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    var jwtOptions =
                        builder.Configuration.GetSection(JwtOptions.JwtOptionsKey).Get<JwtOptions>()
                        ?? throw new ArgumentException(nameof(JwtOptions));

                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.Secret)
                        ),
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies[
                                AuthConstants.AccessToken
                            ];
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("AUTH FAILED: " + context.Exception.Message);
                            return Task.CompletedTask;
                        },
                    };
                }
            );

        // This prevents JwtSecurityHandler from remapping JWT claim names into old
        // WS-Federation format. (which caused a bug I lost 6 hours of my life to (^_^)
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        // Add Authorization
        builder.Services.AddAuthorization();

        return builder;
    }

    /// <summary>
    /// Adds miscellaneous services to the container.
    /// </summary>
    /// <remarks>
    /// Since any service that's needed for handling requests is registered by classes
    /// that Implement <see cref="IEndpoints"/>, this method should be used as a central place to
    /// add any dependency that's doesn't fit that category (AKA, Miscellaneous Service).
    /// </remarks>
    public static WebApplicationBuilder AddMiscServices(this WebApplicationBuilder builder)
    {
        return builder;
    }

    /// <summary>
    /// Scans everything in the `Program`'s assembly, finds every class that implements
    /// `IEndpoints` interface and dynamically calls their `IEndpoints.AddServices()` method.
    /// Effectively adds all endpoints and their required services to the application.
    /// </summary>
    public static WebApplicationBuilder AddApplicationEndpoints(this WebApplicationBuilder builder)
    {
        builder.Services.AddApplicationEndpoints<Program>(builder.Configuration);

        return builder;
    }
}
