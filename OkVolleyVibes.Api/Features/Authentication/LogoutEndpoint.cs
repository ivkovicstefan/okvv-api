using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Application.Authentication.Logout;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Api.Features.Authentication;

public sealed class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/logout", Handle)
            .WithTags("Authentication")
            .WithSummary("Revoke a refresh token. Always 204.")
            .AllowAnonymous()
            .RequireRateLimiting(DependencyInjection.AuthRateLimitPolicy)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

    private static async Task<IResult> Handle(
        LogoutCommand command, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }
}
