using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Authentication.ResendVerification;

internal sealed class ResendVerificationHandler(
    IIdentityService identity,
    IEmailSender email,
    ITranslator translator,
    IAuthLinkBuilder links) : IRequestHandler<ResendVerificationCommand, Unit>
{
    public async Task<Unit> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        Guid? userId = await identity.FindUserIdByEmailAsync(request.Email, cancellationToken);

        // Silent no-op when the account is unknown or already confirmed — never reveal which.
        if (userId is null || await identity.IsEmailConfirmedAsync(userId.Value, cancellationToken))
        {
            return Unit.Value;
        }

        string token = await identity.GenerateEmailConfirmationTokenAsync(userId.Value, cancellationToken);
        string link = links.EmailConfirmationLink(userId.Value, token);

        await email.SendAsync(
            new EmailMessage(
                request.Email,
                translator["Email.Verify.Subject"],
                translator.Format("Email.Verify.Body", link)),
            cancellationToken);

        return Unit.Value;
    }
}
