using Microsoft.EntityFrameworkCore;
using OkVolleyVibes.Application.Common.Abstractions;

namespace OkVolleyVibes.Infrastructure.Persistence;

internal sealed class DbUserPhotoStore(AppDbContext db, IClock clock) : IUserPhotoStore
{
    public async Task SaveAsync(Guid userId, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        UserPhoto? existing = await db.Set<UserPhoto>()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (existing is null)
        {
            db.Set<UserPhoto>().Add(new UserPhoto
            {
                UserId = userId,
                Content = content,
                ContentType = contentType,
                UploadedAtUtc = clock.UtcNow,
            });
        }
        else
        {
            existing.Content = content;
            existing.ContentType = contentType;
            existing.UploadedAtUtc = clock.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<StoredPhoto?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        UserPhoto? photo = await db.Set<UserPhoto>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        return photo is null ? null : new StoredPhoto(photo.Content, photo.ContentType);
    }

    public Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken)
        => db.Set<UserPhoto>().AnyAsync(p => p.UserId == userId, cancellationToken);
}
