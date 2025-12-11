using AuthTemplate;
using AuthTemplate.Endpoints.Internal;
using Scalar.AspNetCore;
using Serilog;

// THINGS I'VE ALREADY ADDED
//  Custom Exceptions and Global Exception Handling for Common Scenarios
//  JWT Authentication with Role-based Authorization (Current Available Roles: [Admin, User])
//  Extendable CORS Configuration (see appsettings.json)
//  Structured Logging with Serilog
//  Central Package Management
//  Strict Code Analysing
//  Validation with FluentValidation
//  Fully Documented Enpoints For Auth, Profile and Admin Ops
//      and their respective Service Layer Abstractions and Implementation
//  Configuration Control from appsettings.json
//  More ...

// THINGS TO CONSIDER ADDING
//  Containerization Support
//  Rate Limiting and Throttling for Lax CORS Policies
//  Caching
//  HTTPS Configuration for Production Environments

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder
    .BindApplicationOptions()
    .ConfigureLogging()
    .ConfigureApiDocs()
    .ConfigureCors()
    .ConfigureErrorHandling()
    .ConfigureValidation()
    .ConfigureDatabase()
    .ConfigureAuth()
    .AddMiscServices()
    .AddApplicationEndpoints();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.ApplyMigrationsAsync();
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("API Template With Auth");
    });
}

app.UseExceptionHandler(_ => { });
app.UseSerilogRequestLogging();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseApplicationEndpoints<Program>();

await app.EnsureRolesCreatedAsync(); // Ensure AuthRolenamesConstants.AsList exist in db
await app.EnsureAdminCreatedAsync(); // Create the initial admin

await app.RunAsync();
