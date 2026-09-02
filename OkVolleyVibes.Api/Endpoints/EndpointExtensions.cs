using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OkVolleyVibes.Api.Endpoints;

public static class EndpointExtensions
{
    /// <summary>Discovers every <see cref="IEndpoint"/> in this assembly and registers it.</summary>
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        ServiceDescriptor[] descriptors = Assembly.GetExecutingAssembly()
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false }
                           && type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(descriptors);

        return services;
    }

    /// <summary>Maps every registered <see cref="IEndpoint"/> onto the route table.</summary>
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}
