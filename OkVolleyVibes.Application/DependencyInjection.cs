using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OkVolleyVibes.Application.Common.Behaviors;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Application layer: the mediator (handlers scanned from this assembly),
    /// the request pipeline, and FluentValidation validators.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(ApplicationAssemblyReference.Assembly);

        // Registration order = execution order (outermost first): logging wraps validation wraps the handler.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddValidatorsFromAssembly(ApplicationAssemblyReference.Assembly, includeInternalTypes: true);

        return services;
    }
}
