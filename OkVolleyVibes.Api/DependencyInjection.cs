using System.Diagnostics;
using System.Threading.RateLimiting;
using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Api.ExceptionHandling;

namespace OkVolleyVibes.Api;

public static class DependencyInjection
{
    /// <summary>Supported UI cultures. First entry is the default.</summary>
    public static readonly string[] SupportedCultures = ["en", "sr-Latn", "ru"];

    /// <summary>Rate-limiter policy name applied to the authentication endpoints.</summary>
    public const string AuthRateLimitPolicy = "auth";

    /// <summary>Registers presentation-layer services (endpoints, localization, rate limiting, OpenAPI, error handling).</summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddEndpoints();

        services.AddHealthChecks();
        services.AddOpenApi();

        services.AddRequestLocalization(options =>
        {
            options.SetDefaultCulture(SupportedCultures[0])
                .AddSupportedCultures(SupportedCultures)
                .AddSupportedUICultures(SupportedCultures);
            options.ApplyCurrentCultureToResponseHeaders = true;
        });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AuthRateLimitPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(5),
                        PermitLimit = 10,
                        QueueLimit = 0,
                    }));
        });

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
        app.UseRequestLocalization();
        app.UseRateLimiter();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "OK Volley Vibes API v1");
                options.DocumentTitle = "OK Volley Vibes API";
            });
        }

        app.MapEndpoints();

        return app;
    }
}
