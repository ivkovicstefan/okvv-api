namespace OkVolleyVibes.Infrastructure.Persistence;

/// <summary>A user's profile photo, stored in the database (one per user). Move to object storage later.</summary>
internal sealed class UserPhoto
{
    public Guid UserId { get; set; }

    public byte[] Content { get; set; } = [];

    public string ContentType { get; set; } = string.Empty;

    public DateTime UploadedAtUtc { get; set; }
}
