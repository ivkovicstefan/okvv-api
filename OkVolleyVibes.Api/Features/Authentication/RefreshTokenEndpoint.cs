using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Application.Authentication.RefreshToken;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Api.Features.Authentication;

public sealed class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/refresh", Handle)
            .WithTags("Authentication")
            .WithSummary("Exchange a refresh token for a fresh access + refresh pair.")
            .AllowAnonymous()
            .RequireRateLimiting(DependencyInjection.AuthRateLimitPolicy)
            .Produces<AuthTokens>()
            .ProducesValidationProblem();

    private static async Task<IResult> Handle(
        RefreshTokenCommand command, ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(command, cancellationToken));
}
