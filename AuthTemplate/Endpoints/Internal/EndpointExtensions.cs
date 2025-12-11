using System.Reflection;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthTemplate.Endpoints.Internal;

public static class EndpointExtensions
{
    /// <summary>
    /// Scans everything in the given assembly, finds every class that implements
    /// `IEndpoints` interface and dynamically calls their `IEndpoints.AddServices(IServiceCollection, IConfiguration)` method.
    /// </summary>
    public static void AddApplicationEndpoints<TMarker>(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        AddApplicationEndpoints(services, typeof(TMarker), configuration);
    }

    /// <summary>
    /// Scans everything in the given assembly, finds every class that implements
    /// `IEndpoints` interface and dynamically calls their `IEndpoints.AddServices(IServiceCollection, IConfiguration)` method.
    /// </summary>
    public static void AddApplicationEndpoints(
        this IServiceCollection services,
        Type typeMarker,
        IConfiguration configuration
    )
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

        foreach (var endpointType in endpointTypes)
        {
            endpointType
                .GetMethod(nameof(IEndpoints.AddServices))!
                .Invoke(null, [services, configuration]);
        }
    }

    /// <summary>
    /// Scans everything in the given assembly, finds every class that implements
    /// `IEndpoints` interface and dynamically calls their
    /// `IEndpoints.DefineEndpoints(IEndpointRouteBuilder)` method.
    /// </summary>
    public static void UseApplicationEndpoints<TMarker>(this IApplicationBuilder app)
    {
        UseApplicationEndpoints(app, typeof(TMarker));
    }

    /// <summary>
    /// Scans everything in the given assembly, finds every class that implements
    /// `IEndpoints` interface and dynamically calls their
    /// `IEndpoints.DefineEndpoints(IEndpointRouteBuilder)` method.
    /// </summary>
    public static void UseApplicationEndpoints(this IApplicationBuilder app, Type typeMarker)
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IEndpoints.DefineEndpoints))!.Invoke(null, [app]);
        }
    }

    private static IEnumerable<TypeInfo> GetEndpointTypesFromAssemblyContaining(Type typeMarker)
    {
        return typeMarker.Assembly.DefinedTypes.Where(x =>
            !x.IsAbstract && !x.IsInterface && typeof(IEndpoints).IsAssignableFrom(x)
        );
    }
}
