namespace OkVolleyVibes.Application.Common.Abstractions;

/// <summary>Issues, rotates and revokes the API's own access + refresh tokens.</summary>
public interface ITokenService
{
    /// <summary>Issues a fresh access + refresh pair for the user.</summary>
    Task<AuthTokens> IssueAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Validates a refresh token, rotates it, and returns a new pair. Throws <c>ValidationException</c> if it is unknown, expired or revoked.</summary>
    Task<AuthTokens> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>Revokes a single refresh token (sign out on this device). No-op if already gone.</summary>
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>Revokes every refresh token for the user (sign out everywhere).</summary>
    Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken);
}

public sealed record AuthTokens(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAtUtc);
