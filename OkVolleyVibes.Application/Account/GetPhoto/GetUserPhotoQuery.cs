using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Domain.Common.Exceptions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Account.GetPhoto;

/// <summary>Fetches a member's profile photo. Any signed-in user may view any member's photo.</summary>
public sealed record GetUserPhotoQuery(Guid UserId) : IRequest<StoredPhoto>;

internal sealed class GetUserPhotoHandler(IUserPhotoStore photos)
    : IRequestHandler<GetUserPhotoQuery, StoredPhoto>
{
    public async Task<StoredPhoto> Handle(GetUserPhotoQuery request, CancellationToken cancellationToken)
        => await photos.GetAsync(request.UserId, cancellationToken)
           ?? throw new NotFoundException("Photo", request.UserId);
}
