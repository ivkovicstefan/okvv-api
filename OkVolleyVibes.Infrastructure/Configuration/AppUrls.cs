namespace OkVolleyVibes.Infrastructure.Configuration;

/// <summary>Bound from the <c>App</c> configuration section.</summary>
public sealed class AppUrls
{
    public const string SectionName = "App";

    /// <summary>Public base URL of this API, used to build links in emails.</summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:5080";
}
