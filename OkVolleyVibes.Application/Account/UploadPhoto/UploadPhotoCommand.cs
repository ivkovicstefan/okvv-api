using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Account.UploadPhoto;

/// <summary>Sets (or replaces) the signed-in user's profile photo.</summary>
public sealed record UploadPhotoCommand(byte[] Content, string ContentType) : IRequest<Unit>;
