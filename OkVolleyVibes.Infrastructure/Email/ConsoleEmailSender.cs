using Microsoft.Extensions.Logging;
using OkVolleyVibes.Application.Common.Abstractions;

namespace OkVolleyVibes.Infrastructure.Email;

/// <summary>Development email sender: writes the message to the log instead of sending it.</summary>
internal sealed class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[email] to <{To}> | {Subject}\n{Body}",
            message.To,
            message.Subject,
            message.HtmlBody);

        return Task.CompletedTask;
    }
}
