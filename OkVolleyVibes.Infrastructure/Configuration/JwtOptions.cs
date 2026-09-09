namespace OkVolleyVibes.Infrastructure.Configuration;

/// <summary>Bound from the <c>Jwt</c> configuration section.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "okvv-api";

    public string Audience { get; set; } = "okvv-clients";

    /// <summary>HMAC-SHA256 signing key — at least 32 bytes. Required; no default in Production.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 30;
}
