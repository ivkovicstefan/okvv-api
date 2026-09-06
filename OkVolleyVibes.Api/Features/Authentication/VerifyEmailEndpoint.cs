using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Application.Authentication.VerifyEmail;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Api.Features.Authentication;

public sealed class VerifyEmailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/api/auth/verify-email", Handle)
            .WithTags("Authentication")
            .WithSummary("Confirm an email address from the link in the verification email.")
            .RequireRateLimiting(DependencyInjection.AuthRateLimitPolicy)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        Guid userId, string token, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new VerifyEmailCommand(userId, token), cancellationToken);
        return Results.NoContent();
    }
}
