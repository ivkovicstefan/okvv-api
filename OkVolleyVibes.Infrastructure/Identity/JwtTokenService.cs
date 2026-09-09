using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Domain.Common.Exceptions;
using OkVolleyVibes.Infrastructure.Configuration;
using OkVolleyVibes.Infrastructure.Persistence;

namespace OkVolleyVibes.Infrastructure.Identity;

internal sealed class JwtTokenService(
    AppDbContext db,
    UserManager<User> userManager,
    IOptions<JwtOptions> options,
    IClock clock) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public async Task<AuthTokens> IssueAsync(Guid userId, CancellationToken cancellationToken)
    {
        User user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        return await IssueForAsync(user, cancellationToken);
    }

    public async Task<AuthTokens> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        string hash = Hash(refreshToken);
        DateTime now = clock.UtcNow;

        RefreshToken? stored = await db.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (stored is null || stored.RevokedAtUtc is not null || stored.ExpiresAtUtc <= now)
        {
            throw new ValidationException("refreshToken", "The refresh token is invalid or has expired.");
        }

        stored.RevokedAtUtc = now;

        User user = await userManager.FindByIdAsync(stored.UserId.ToString())
            ?? throw new NotFoundException("User", stored.UserId);

        return await IssueForAsync(user, cancellationToken);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken)
    {
        string hash = Hash(refreshToken);

        RefreshToken? stored = await db.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAtUtc == null, cancellationToken);

        if (stored is not null)
        {
            stored.RevokedAtUtc = clock.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken)
        => db.Set<RefreshToken>()
            .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAtUtc, clock.UtcNow), cancellationToken);

    private async Task<AuthTokens> IssueForAsync(User user, CancellationToken cancellationToken)
    {
        DateTime now = clock.UtcNow;
        DateTime accessExpiresAt = now.AddMinutes(_options.AccessTokenMinutes);
        IList<string> roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(AppClaimTypes.Subject, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(AppClaimTypes.Email, user.Email ?? string.Empty),
            new(AppClaimTypes.ProfileCompleted, user.ProfileCompletedAtUtc is not null ? "true" : "false"),
        };
        claims.AddRange(roles.Select(role => new Claim(AppClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now,
            NotBefore = now,
            Expires = accessExpiresAt,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
        };

        string accessToken = new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false }.CreateToken(descriptor);

        string rawRefreshToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        db.Set<RefreshToken>().Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = Hash(rawRefreshToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(_options.RefreshTokenDays),
        });

        await db.SaveChangesAsync(cancellationToken);

        return new AuthTokens(accessToken, rawRefreshToken, accessExpiresAt);
    }

    private static string Hash(string value)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
