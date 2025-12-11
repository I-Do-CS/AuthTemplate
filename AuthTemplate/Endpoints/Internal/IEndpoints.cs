namespace AuthTemplate.Endpoints.Internal;

public interface IEndpoints
{
    static abstract void AddServices(IServiceCollection services, IConfiguration configuration);
    static abstract void DefineEndpoints(IEndpointRouteBuilder app);
}
