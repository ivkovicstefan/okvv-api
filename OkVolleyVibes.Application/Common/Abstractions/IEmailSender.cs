namespace OkVolleyVibes.Application.Common.Abstractions;

/// <summary>Sends a transactional email. Dev implementation writes to the log; prod uses a provider.</summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}

public sealed record EmailMessage(string To, string Subject, string HtmlBody);
