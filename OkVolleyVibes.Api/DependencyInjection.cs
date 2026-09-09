using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using OkVolleyVibes.Api.Auth;
using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Api.ExceptionHandling;
using OkVolleyVibes.Application.Account.UploadPhoto;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Domain.Common;

namespace OkVolleyVibes.Api;

public static class DependencyInjection
{
    /// <summary>Rate-limiter policy applied to the authentication endpoints.</summary>
    public const string AuthRateLimitPolicy = "auth";

    /// <summary>Policy for endpoints that need a signed-in user whose onboarding is finished.</summary>
    public const string ProfileCompletePolicy = "ProfileComplete";

    /// <summary>Registers presentation-layer services (auth, endpoints, localization, rate limiting, OpenAPI, error handling).</summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddEndpoints();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // Cap multipart uploads a little above the profile-photo limit.
        services.Configure<FormOptions>(options =>
            options.MultipartBodyLengthLimit = PhotoLimits.MaxBytes + (64 * 1024));

        services.AddHealthChecks();
        services.AddOpenApi();

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy(ProfileCompletePolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.ProfileCompleted, "true"));

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, ProblemDetailsAuthorizationResultHandler>();

        string[] cultures = [.. Language.Supported];
        services.AddRequestLocalization(options =>
        {
            options.SetDefaultCulture(Language.Default)
                .AddSupportedCultures(cultures)
                .AddSupportedUICultures(cultures);
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

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseRateLimiter();
        }

        app.UseAuthentication();
        app.UseAuthorization();

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
