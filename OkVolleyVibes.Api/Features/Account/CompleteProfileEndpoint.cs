using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Application.Onboarding.CompleteProfile;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Api.Features.Account;

public sealed class CompleteProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/api/account/complete-profile", Handle)
            .WithTags("Account")
            .WithSummary("Submit the onboarding survey. Returns a fresh token pair once complete.")
            .RequireAuthorization()
            .Produces<AuthTokens>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle(
        CompleteProfileCommand command, ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(command, cancellationToken));
}
