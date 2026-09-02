using OkVolleyVibes.Api.Endpoints;

namespace OkVolleyVibes.Api.Features.Health;

/// <summary>Liveness probe. Returns <c>200 Healthy</c> while the API process is running.</summary>
public sealed class HealthEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapHealthChecks("/health")
            .WithName("HealthCheck")
            .WithTags("System")
            .WithSummary("Liveness probe");
}
