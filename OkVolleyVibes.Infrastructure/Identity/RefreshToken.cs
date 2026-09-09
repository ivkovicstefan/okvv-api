namespace OkVolleyVibes.Infrastructure.Identity;

/// <summary>A refresh token, stored only as a SHA-256 hash. Rotated on use; revoked on sign-out.</summary>
internal sealed class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }
}
