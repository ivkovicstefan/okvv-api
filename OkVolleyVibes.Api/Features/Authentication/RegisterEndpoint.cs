using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Application.Authentication.Register;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Api.Features.Authentication;

public sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/register", Handle)
            .WithTags("Authentication")
            .WithSummary("Register a new account (assigned the Player role; a verification email is sent).")
            .RequireRateLimiting(DependencyInjection.AuthRateLimitPolicy)
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle(RegisterCommand command, ISender sender, CancellationToken cancellationToken)
    {
        RegisterResponse result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/users/{result.UserId}", result);
    }
}
