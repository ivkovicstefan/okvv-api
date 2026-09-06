using System.Text.RegularExpressions;
using OkVolleyVibes.Application.Common.Abstractions;

namespace OkVolleyVibes.Tests.Authentication;

public sealed partial class FakeEmailSender : IEmailSender
{
    private readonly List<EmailMessage> _sent = [];

    public IReadOnlyList<EmailMessage> Sent => _sent;

    public int CountTo(string email) => _sent.Count(m => string.Equals(m.To, email, StringComparison.OrdinalIgnoreCase));

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        _sent.Add(message);
        return Task.CompletedTask;
    }

    /// <summary>Path + query of the verify-email link in the most recent message to <paramref name="email"/>.</summary>
    public string VerificationPathFor(string email)
    {
        EmailMessage message = _sent.LastOrDefault(m => string.Equals(m.To, email, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No email was sent to {email}.");

        Match match = LinkPattern().Match(message.HtmlBody);
        if (!match.Success)
        {
            throw new InvalidOperationException("No verification link found in the email body.");
        }

        return new Uri(match.Value).PathAndQuery;
    }

    [GeneratedRegex(@"http[^""\s<)]+/api/auth/verify-email\?[^""\s<)]+")]
    private static partial Regex LinkPattern();
}
