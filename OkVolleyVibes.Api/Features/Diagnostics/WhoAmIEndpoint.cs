using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Application.Common.Abstractions;

namespace OkVolleyVibes.Api.Features.Diagnostics;

/// <summary>
/// Development/Testing only. <c>GET /_diag/whoami</c> is protected by the <c>ProfileComplete</c>
/// policy, so it also serves as a probe for the onboarding gate.
/// </summary>
public sealed class WhoAmIEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        IHostEnvironment env = app.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (!env.IsDevelopment() && !env.IsEnvironment("Testing"))
        {
            return;
        }

        app.MapGet("/_diag/whoami", (ICurrentUser me) => Results.Ok(new
            {
                userId = me.UserId,
                roles = me.Roles,
                profileCompleted = me.ProfileCompleted,
            }))
            .WithTags("Diagnostics")
            .RequireAuthorization(DependencyInjection.ProfileCompletePolicy)
            .ExcludeFromDescription();
    }
}
