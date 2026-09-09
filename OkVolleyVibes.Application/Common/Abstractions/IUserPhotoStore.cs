namespace OkVolleyVibes.Application.Common.Abstractions;

/// <summary>Stores and retrieves a user's profile photo. Backed by the database for now; object storage later.</summary>
public interface IUserPhotoStore
{
    Task SaveAsync(Guid userId, byte[] content, string contentType, CancellationToken cancellationToken);

    Task<StoredPhoto?> GetAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken);
}

public sealed record StoredPhoto(byte[] Content, string ContentType);
