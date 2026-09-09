using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Account.UploadPhoto;

internal sealed class UploadPhotoHandler(ICurrentUser currentUser, IUserPhotoStore photos)
    : IRequestHandler<UploadPhotoCommand, Unit>
{
    public async Task<Unit> Handle(UploadPhotoCommand request, CancellationToken cancellationToken)
    {
        await photos.SaveAsync(
            currentUser.RequireUserId(), request.Content, request.ContentType, cancellationToken);

        return Unit.Value;
    }
}
