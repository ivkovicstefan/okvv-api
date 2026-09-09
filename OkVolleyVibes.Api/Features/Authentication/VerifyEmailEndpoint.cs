using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Application.Authentication.VerifyEmail;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Api.Features.Authentication;

public sealed class VerifyEmailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/api/auth/verify-email", Handle)
            .WithTags("Authentication")
            .WithSummary("Confirm an email address and sign the user in (returns a token pair).")
            .RequireRateLimiting(DependencyInjection.AuthRateLimitPolicy)
            .Produces<AuthTokens>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        Guid userId, string token, ISender sender, CancellationToken cancellationToken)
    {
        AuthTokens tokens = await sender.Send(new VerifyEmailCommand(userId, token), cancellationToken);
        return Results.Ok(tokens);
    }
}
