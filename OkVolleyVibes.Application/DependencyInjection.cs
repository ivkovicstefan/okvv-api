using Microsoft.Extensions.DependencyInjection;

namespace OkVolleyVibes.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Application layer: use-case handlers, validators, and pipeline behaviors.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register handlers / validators / pipeline behaviors here as features are added.
        return services;
    }
}
