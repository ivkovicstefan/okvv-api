namespace OkVolleyVibes.Domain.Players;

/// <summary>
/// Volleyball &amp; membership data for a user who holds the <c>Player</c> role. One per user (1:1),
/// linked by <see cref="UserId"/> — identity itself lives in Infrastructure. Created when the
/// <c>Player</c> role is granted (see FR-B5); most fields arrive later when FR-C is scoped.
/// </summary>
public sealed class PlayerProfile
{
    private PlayerProfile()
    {
        // EF Core
    }

    private PlayerProfile(Guid userId, DateTime createdAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAtUtc = createdAtUtc;
        MembershipStatus = MembershipStatus.Pending;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public MembershipStatus MembershipStatus { get; private set; }

    public static PlayerProfile Create(Guid userId, DateTime createdAtUtc) => new(userId, createdAtUtc);
}
