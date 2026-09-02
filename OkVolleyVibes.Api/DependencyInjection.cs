using System.Diagnostics;
using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Api.ExceptionHandling;

namespace OkVolleyVibes.Api;

public static class DependencyInjection
{
    /// <summary>Registers presentation-layer services (endpoints, health checks, OpenAPI, error handling).</summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddEndpoints();

        services.AddHealthChecks();
        services.AddOpenApi();

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance ??=
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                context.ProblemDetails.Extensions["traceId"] =
                    Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
            };
        });

        // Ordered: AppException first, catch-all last.
        services.AddExceptionHandler<AppExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

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
