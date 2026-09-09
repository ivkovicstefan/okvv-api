using System.Globalization;
using System.Text.Json;
using OkVolleyVibes.Application.Common.Abstractions;

namespace OkVolleyVibes.Infrastructure.Localization;

/// <summary>
/// <see cref="ITranslator"/> backed by embedded JSON files (<c>strings.{culture}.json</c>).
/// Resolves against the current UI culture, then its parent, then English, then the key itself.
/// </summary>
internal sealed class JsonTranslator : ITranslator
{
    private const string DefaultCulture = "en";
    private static readonly string[] Cultures = ["en", "sr-Latn", "ru"];

    private readonly Dictionary<string, Dictionary<string, string>> _byCulture;

    public JsonTranslator()
    {
        _byCulture = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        var assembly = typeof(JsonTranslator).Assembly;
        foreach (string culture in Cultures)
        {
            string resource = $"OkVolleyVibes.Infrastructure.Localization.strings-{culture}.json";
            using Stream? stream = assembly.GetManifestResourceStream(resource);
            if (stream is null)
            {
                continue;
            }

            using var reader = new StreamReader(stream);
            var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd());
            _byCulture[culture] = parsed ?? [];
        }
    }

    public string this[string key]
    {
        get
        {
            CultureInfo culture = CultureInfo.CurrentUICulture;

            if (TryGet(culture.Name, key, out string value)
                || TryGet(culture.Parent.Name, key, out value)
                || TryGet(DefaultCulture, key, out value))
            {
                return value;
            }

            return key;
        }
    }

    public string Format(string key, params object[] args)
        => string.Format(CultureInfo.CurrentUICulture, this[key], args);

    private bool TryGet(string culture, string key, out string value)
    {
        if (!string.IsNullOrEmpty(culture)
            && _byCulture.TryGetValue(culture, out Dictionary<string, string>? map)
            && map.TryGetValue(key, out string? found))
        {
            value = found;
            return true;
        }

        value = string.Empty;
        return false;
    }
}
