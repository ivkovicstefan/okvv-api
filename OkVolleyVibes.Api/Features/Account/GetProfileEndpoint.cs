using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Application.Account.GetProfile;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Api.Features.Account;

public sealed class GetProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/api/account/profile", Handle)
            .WithTags("Account")
            .WithSummary("The signed-in user's own profile and onboarding status.")
            .RequireAuthorization()
            .Produces<ProfileResponse>();

    private static async Task<IResult> Handle(ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new GetProfileQuery(), cancellationToken));
}
