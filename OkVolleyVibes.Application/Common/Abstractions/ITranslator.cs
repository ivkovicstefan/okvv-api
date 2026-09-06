namespace OkVolleyVibes.Application.Common.Abstractions;

/// <summary>
/// Culture-aware string lookup for server-produced text (validation messages, emails).
/// Resolves against <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>, then English,
/// then returns the key itself. Supported cultures: <c>en</c>, <c>sr-Latn</c>, <c>ru</c>.
/// </summary>
public interface ITranslator
{
    string this[string key] { get; }

    string Format(string key, params object[] args);
}
