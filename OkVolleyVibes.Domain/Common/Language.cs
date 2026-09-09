namespace OkVolleyVibes.Domain.Common;

/// <summary>Languages the platform supports for server-produced text and user preferences.</summary>
public static class Language
{
    public const string Default = "en";

    /// <summary>BCP-47 tags. Serbian is Latin script.</summary>
    public static readonly IReadOnlyList<string> Supported = ["en", "sr-Latn", "ru"];

    public static bool IsSupported(string? tag)
        => tag is not null && Supported.Contains(tag);
}
