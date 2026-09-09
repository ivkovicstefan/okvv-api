using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Application.Account.GetPhoto;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Api.Features.Account;

public sealed class GetPhotoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/api/account/photo/{userId:guid}", Handle)
            .WithTags("Account")
            .WithSummary("A member's profile photo.")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK, contentType: "image/jpeg")
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(Guid userId, ISender sender, CancellationToken cancellationToken)
    {
        StoredPhoto photo = await sender.Send(new GetUserPhotoQuery(userId), cancellationToken);
        return Results.File(photo.Content, photo.ContentType);
    }
}
