using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Application.Diagnostics.Ping;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Api.Features.Diagnostics;

/// <summary>
/// Development/Testing only. <c>GET /_diag/ping?message=...</c> exercises the full mediator
/// pipeline (endpoint → ISender → LoggingBehavior → ValidationBehavior → handler). Excluded
/// from the OpenAPI document.
/// </summary>
public sealed class PingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        IHostEnvironment env = app.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (!env.IsDevelopment() && !env.IsEnvironment("Testing"))
        {
            return;
        }

        app.MapGet("/_diag/ping", async (string? message, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new PingQuery(message), ct)))
            .WithTags("Diagnostics")
            .ExcludeFromDescription();
    }
}
