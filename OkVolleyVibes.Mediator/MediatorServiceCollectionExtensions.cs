using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OkVolleyVibes.Mediator;

public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ISender"/> and every <see cref="IRequestHandler{TRequest,TResponse}"/>
    /// found in the given assemblies. Pipeline behaviors are registered separately by the caller,
    /// as open generics, in the order they should run (first = outermost).
    /// </summary>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            throw new ArgumentException("Provide at least one assembly to scan.", nameof(assemblies));
        }

        services.TryAddScoped<ISender, Sender>();

        Type openHandler = typeof(IRequestHandler<,>);

        IEnumerable<(Type Service, Type Implementation)> registrations =
            from type in assemblies.SelectMany(a => a.DefinedTypes)
            where type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false }
            from @interface in type.ImplementedInterfaces
            where @interface.IsGenericType && @interface.GetGenericTypeDefinition() == openHandler
            select ((Type)@interface, type.AsType());

        foreach ((Type service, Type implementation) in registrations)
        {
            services.TryAddScoped(service, implementation);
        }

        return services;
    }
}
