using OkVolleyVibes.Api.Endpoints;

namespace OkVolleyVibes.Api;

public static class DependencyInjection
{
    /// <summary>Registers presentation-layer services (endpoints, health checks, OpenAPI, ProblemDetails).</summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddEndpoints();

        services.AddHealthChecks();
        services.AddOpenApi();
        services.AddProblemDetails();

        return services;
    }

    /// <summary>Wires the HTTP pipeline and maps all discovered endpoints.</summary>
    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapEndpoints();

        return app;
    }
}
