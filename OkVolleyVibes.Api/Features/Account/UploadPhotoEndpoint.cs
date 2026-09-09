using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Application.Account.UploadPhoto;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Api.Features.Account;

public sealed class UploadPhotoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/api/account/photo", Handle)
            .WithTags("Account")
            .WithSummary("Upload (or replace) the signed-in user's profile photo. JPEG/PNG/WebP, ≤ 512 KB.")
            .RequireAuthorization()
            .DisableAntiforgery()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

    private static async Task<IResult> Handle(
        IFormFile file, ISender sender, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);

        await sender.Send(new UploadPhotoCommand(buffer.ToArray(), file.ContentType), cancellationToken);
        return Results.NoContent();
    }
}
