using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Application.Authentication.ResendVerification;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Api.Features.Authentication;

public sealed class ResendVerificationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/resend-verification", Handle)
            .WithTags("Authentication")
            .WithSummary("Re-send the verification email. Always responds 202, regardless of whether the account exists.")
            .RequireRateLimiting(DependencyInjection.AuthRateLimitPolicy)
            .Produces(StatusCodes.Status202Accepted)
            .ProducesValidationProblem();

    private static async Task<IResult> Handle(
        ResendVerificationCommand command, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.Accepted();
    }
}
