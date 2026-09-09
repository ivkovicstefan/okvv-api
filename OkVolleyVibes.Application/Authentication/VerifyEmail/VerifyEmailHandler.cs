using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Authentication.VerifyEmail;

internal sealed class VerifyEmailHandler(IIdentityService identity, ITokenService tokens)
    : IRequestHandler<VerifyEmailCommand, AuthTokens>
{
    public async Task<AuthTokens> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        await identity.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);
        return await tokens.IssueAsync(request.UserId, cancellationToken);
    }
}
